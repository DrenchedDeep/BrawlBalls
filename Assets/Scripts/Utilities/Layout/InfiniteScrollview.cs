using System;
using System.Collections;
using MainMenu.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace Utilities.Layout
{
    public class InfiniteScrollView : ScrollRect//, IPointerClickHandler
    {
        [SerializeField, Tooltip("Insert the background image... This is used to get the width and height")]
        private RectTransform itemTemplate;

        [SerializeField] private bool allowUserControl;
        
        [SerializeField] private bool isSnappy = true;
        [SerializeField] private bool useAnimationCurve;
        [SerializeField] private AnimationCurve animationCurve = new(new Keyframe(0f, 0f), new Keyframe(0.2f, -0.2f), new Keyframe(0.8f, 1.2f), new Keyframe(1f, 1f));
        [SerializeField] private float smoothTime = 0.25f;
        

        private Vector2 _itemSize;
        private int _numItems;
        private int _numItemsOriginally;
        private float _spacing;
        private bool _isSnapping;
        private bool _selected;
        private float _offset;

        private int _numVisible;
        private float _viewportHalfSize;
        private float _itemScreenSpacing;
        private float _viewportOffset;
        
        private int _currentItemNum = Int32.MaxValue;
        private int _currentSelectedNum = Int32.MaxValue;
        private IInfiniteScrollItem[] _items;
        
        
        

        private Coroutine _snap;
        
        
        
        
        protected override void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            base.Start();

            _itemSize = ((RectTransform)content.GetChild(0)).rect.size;

            
            
            

#if UNITY_EDITOR
            if (content.childCount == 0)
            {
                Debug.LogWarning("Empty InfiniteScrollView is disabling itself", gameObject);
                if(Application.isPlaying) enabled = false;
                return;
            }
      
            foreach (RectTransform rt in content)
            {
                if (_itemSize != rt.rect.size)
                {
                    Debug.LogWarning("Infinite Scroll View Currently only supports items of matching scale: ", rt.gameObject);
                }
            }
            #endif
            
            //Get the number of child objects...
            
             HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            _spacing = horizontalOrVerticalLayoutGroup.spacing;
            _offset = -viewport.anchoredPosition.y;
            
            _numItemsOriginally = content.childCount;
            _numItems = _numItemsOriginally;
            
            _items = new IInfiniteScrollItem[_numItems];
            for(int i= 0; i< _numItems; ++i)
            {
                _items[i] = content.GetChild(i).GetComponent<IInfiniteScrollItem>();
                if (_items[i] == null)
                {
                    Debug.LogWarning("An item is not marked as infinite scroll and will not work properly: ", content.GetChild(0));
                }
            }
            
            
            FillScrollArea();

            _numItems = content.childCount;
            
            _items = new IInfiniteScrollItem[_numItems];
            for(int i= 0; i< _numItems; ++i)
            {
                _items[i] = content.GetChild(i).GetComponent<IInfiniteScrollItem>();
                if (_items[i] == null)
                {
                    Debug.LogWarning("An item is not marked as infinite scroll and will not work properly: ", content.GetChild(0));
                }
            }
            
            
            CreateIllusion();

            

            _currentItemNum = GetCurrentNum();
            _currentSelectedNum = _currentItemNum;
            _items[_currentSelectedNum].OnSelected();
            //Why the fuck?
            
            
            //size.y = _itemSize.y * _numItems + vlg.spacing * (_numItems - 1) + vlg.padding.vertical;
            _viewportOffset = _numItems * ((_itemSize.y + _spacing)/2); // 10 * 
                
            Debug.Log($"Viewport offset: {_viewportOffset} = {_numItems} * ({_itemSize.y} + {_spacing})");
            
#if UNITY_EDITOR
            WhyNotDeleting();
#endif
        }
        
        protected override void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            base.LateUpdate();

            if (isSnappy && !_isSnapping && !_selected  && velocity.sqrMagnitude <= 15) _snap=StartCoroutine(Snap());
            
            if (velocity.y < 0 && content.localPosition.y < -_viewportOffset + _itemScreenSpacing) //TOP: -95 * num items..
            { 
                content.localPosition =  new Vector3(content.localPosition.x,  _viewportOffset + _itemScreenSpacing);
                OnEndDrag(new PointerEventData(EventSystem.current));
            }
            else if (velocity.y > 0 && content.localPosition.y > _viewportOffset - _offset + _itemScreenSpacing)
            {
                // Overscrolled at the bottom: clamp to bottom limit.
                content.localPosition = new Vector3(content.localPosition.x, -_viewportOffset + _offset + _itemScreenSpacing);
                OnEndDrag(new PointerEventData(EventSystem.current));
            }

            int itemNum = GetCurrentNum();
            
            
            Debug.Log($"itemNum: {itemNum} => {(content.localPosition.y)}+{(_offset + _viewportHalfSize)} / {(_itemSize.y + _spacing)}");

            //return;
            if (itemNum != _currentItemNum)
            {
                _items[itemNum].OnHover();
                _items[_currentItemNum].OnUnHover();
                _currentItemNum = itemNum;
            }
        }
        
        
        // 10 (0 --> -580), 


        private int GetCurrentNum()
        {
            // Each item occupies its height plus spacing.
            float itemSlot = _itemSize.y + _spacing;
            // Determine the center of the viewport in the content’s coordinate space.
            float centerInContent = content.anchoredPosition.y + _viewportHalfSize;
            // Because CreateIllusion() prepends _numVisible cloned items at the top,
            // subtract their total height to get a coordinate relative to the original items.
            float adjustedCenter = centerInContent - _numVisible * itemSlot;
            // Assuming the original first item’s center is at (_itemSize.y / 2),
            // the index of the centered item is:
            int index = Mathf.RoundToInt((adjustedCenter - (_itemSize.y * 0.5f)) / itemSlot) - _numItems/2;
            // Wrap the index into the range [0, originalCount)
            index = (index % _numItems + _numItems) % _numItems;
            return index;
        }









        //HANDLING INFINITE ITEMS
        //If we have enough, then duplicate THE FIRST ITEMS THAT SHOULD BE VISIBLE (They will be the restarting point)
        //IF WE DO NOT HAVE ENOUGH, Clone until we have enough (Check after cloning a batch, not during the batch)
        //DESTROY CLONES IF EDITOR ENDS
        private void FillScrollArea()
        {
            //We need to determine how many items SHOULD be visible, then we need to determine how many items we have.
            Vector2 viewportSize = viewport.rect.size;
            int numItems = content.childCount;
            
            if (vertical)
            {
                //Calculate the number of items needs, (totalSpacing * totalItemSize) / (ViewportSize + Padding - Spacing) * 2
                float desiredItemSize = _spacing + _itemSize.y; // Let's say is 1
                float currentItemSize = numItems * desiredItemSize; //let's say this is 3
                float scaledViewportSize = (viewportSize.y + _offset - _spacing); // let's say this is 10.
                int numNeeded = Mathf.CeilToInt(Mathf.Max(0,(scaledViewportSize - currentItemSize + 1) / desiredItemSize)); // (10-3)/1 == 7
                _numVisible = Mathf.CeilToInt(scaledViewportSize / desiredItemSize);
                
                
                for (int i = 0; i < numNeeded; ++i)
                {
                    
                    IInfiniteScrollItem clone = Instantiate((MonoBehaviour)_items[i % numItems], content).GetComponent<IInfiniteScrollItem>();
                    clone?.ListenTo(_items[i % numItems]);
                }
                
                Debug.Log($"I need {numNeeded} -> (({viewportSize.y} + {_offset} - {_spacing}) - {currentItemSize}) / {desiredItemSize} items for infinite scrolling");
                
                content.sizeDelta = new Vector2( content.sizeDelta.x , _itemSize.y * content.childCount + (content.childCount-1) * _spacing + _offset);
                _viewportHalfSize = scaledViewportSize / 2;
                _itemScreenSpacing = (Mathf.CeilToInt(_numVisible / 2f) + 1) * (_itemSize.y + _spacing);


            }
            else if (horizontal)
            {
                   // size.x = _itemSize.x * _numItems + hlg.spacing * (_numItems - 1) + hlg.padding.horizontal;
            }
            else
            {
                Debug.LogError("Content not automatically sized as the object: " + content.name +
                               " contains no layout group");
                
            }
            
        }

        private void CreateIllusion()
        {
            //Let's spawn the bottom half required at the top
            int val = _numVisible; // Add additional cushion

            Debug.Log("Creating Illusion with num objects: " + (val * 2));
            
            for (int i = 0; i < val; ++i)
            {
                Debug.Log($"Child: {i*2}, and {content.childCount - i*2 - 1}");
                
          
                
                var a = Instantiate(content.GetChild(content.childCount - i * 2 - 1), content);
                var b = Instantiate(content.GetChild(i * 2), content);

                int t = i % _numItemsOriginally; // 0, 1, 0, 1 | 0, 1, 2, 0
                int k = _numItemsOriginally - t - 1; // 1, 0, 1, 0 | 2, 1, 0, 2
                
                a.GetComponent<IInfiniteScrollItem>()?.ListenTo(_items[k]);
                b.GetComponent<IInfiniteScrollItem>()?.ListenTo(_items[t]);
                
                //b.GetComponent<Image>().color = Color.blue;
                //a.GetComponent<Image>().color = Color.magenta;
                
                a.SetAsFirstSibling();

                a.name = "Clone from end";
                b.name = "Clone from beginning";
            }
        }
        
        
        

        private IEnumerator Snap()
        {
            _selected = true;
            float curTime = 0;
            _isSnapping = true;

            Vector3 localPosition = content.anchoredPosition;
            Vector3 newPos = localPosition;
            float size = _itemSize.y + _spacing;

            // Determine the closest snap target
            float remainder = localPosition.y % size;
            float target;

            if (remainder < size / 2) // Closer to the previous item (snap down)
            {
                target = localPosition.y - remainder - _offset;
            }
            else // Closer to the next item (snap up)
            {
                target = localPosition.y + (size - remainder) - _offset;
            }

            if (useAnimationCurve)
            {
                while (curTime < smoothTime)
                {
                    curTime += Time.deltaTime;
                    newPos.y = Mathf.Lerp(content.anchoredPosition.y, target, animationCurve.Evaluate(curTime / smoothTime));
                    content.anchoredPosition = newPos;
                    yield return null;
                }
            }
            else
            {
                while (curTime < smoothTime)
                {
                    curTime += Time.deltaTime;
                    newPos.y = Mathf.Lerp(content.anchoredPosition.y, target, curTime / smoothTime);
                    content.anchoredPosition = newPos;
                    yield return null;
                }
            }

            // Ensure it locks exactly to the target to avoid precision errors
            newPos.y = target;
            content.anchoredPosition = newPos;

            velocity = Vector2.zero;
            _isSnapping = false;

            if (_currentSelectedNum != _currentItemNum)
            {
                _items[_currentSelectedNum].OnDeselected();
                _items[_currentItemNum].OnSelected();
                _currentSelectedNum = _currentItemNum;
            }
        }




        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            if (_snap != null)
            {
                StopCoroutine(_snap);
                _snap = null;
            }
            _isSnapping = false;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            _selected = false;
        }
        



        #region Control
        public void AddForce(Vector2 amount)
        {
            print("adding force: " + amount);
            velocity += amount;
            _isSnapping = false;
            _selected = false;

            if (_snap != null)
            {
                StopCoroutine(_snap);
                _snap = null;
            }
        }

        
        public void AddRandomForce(float amount)
        {
            AddForce(new Vector2(Random.Range(amount / 10, amount) * (Random.Range(0, 2) == 1 ? -1 : 1),Random.Range(amount / 10, amount) * (Random.Range(0, 2) == 1 ? -1 : 1)));
        }
        /*

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!allowUserControl) return;
            Debug.Log("Registered a click");
            AddForce(new Vector2(horizontal?eventData.position.x - content.rect.width / 2 * 1f:0,vertical?eventData.position.y - content.rect.height / 2 * 1f:0));
        }
        */
        #endregion
        
        #if UNITY_EDITOR

        private void WhyNotDeleting()
        {
            if (Application.isPlaying) return;
    
            for (int i = content.childCount - 1; i >= 0; i--)
            {

                GameObject go = content.GetChild(i).gameObject;

                if(go.name.ToLower().Contains("clone")) DestroyImmediate(go);
            }
        }
        #endif
    }
}
