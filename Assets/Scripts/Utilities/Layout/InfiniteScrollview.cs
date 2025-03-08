using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace Utilities.Layout
{
    public class InfiniteScrollView : ScrollRect, IPointerClickHandler
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
        private float _spacing;
        private bool _isSnapping;
        private bool _selected;
        private float _offset;

        private int _numVisible;
        private float _viewportSize;


        private Coroutine _snap;
        
        
        
        protected override void Start()
        {

            
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
            _offset = horizontalOrVerticalLayoutGroup.padding.vertical;
            
            FillScrollArea();
            _numItems = content.childCount;
            
            Debug.Log("Content Size: " + content.sizeDelta.x + ", " + content.sizeDelta.y);
            CreateIllusion();
            Debug.Log("Content Size: " + content.sizeDelta.x + ", " + content.sizeDelta.y);
            
            //Why the fuck?
#if UNITY_EDITOR
            WhyNotDeleting();
#endif
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
                    Instantiate(content.GetChild(i % numItems), content);
                }
                
                Debug.Log($"I need {numNeeded} -> (({viewportSize.y} + {_offset} - {_spacing}) - {currentItemSize}) / {desiredItemSize} items for infinite scrolling");
                
                content.sizeDelta = new Vector2( content.sizeDelta.x , _itemSize.y * content.childCount + (content.childCount-1) * _spacing + _offset);
                _viewportSize = scaledViewportSize;

                //size.y = _itemSize.y * _numItems + vlg.spacing * (_numItems - 1) + vlg.padding.vertical;
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
            int val = _numVisible;

            Debug.Log("Creating Illusion with num objects: " + (val * 2));
            
            for (int i = 0; i < val; ++i)
            {
                Debug.Log($"Child: {i*2}, and {content.childCount - i*2 - 1}");
                
                var a = Instantiate(content.GetChild(content.childCount - i * 2 - 1), content);
                var b = Instantiate(content.GetChild(i * 2), content);
                
                //b.GetComponent<Image>().color = Color.blue;
                //a.GetComponent<Image>().color = Color.magenta;
                
                a.SetAsFirstSibling();

                a.name = "Clone from end";
                b.name = "Clone from beginning";
            }

            content.localPosition = Vector2.zero;
            
        }
        
        

        private IEnumerator Snap()
        {
            _selected = true;

            float curTime = 0;
            _isSnapping = true;
            
            
            Vector3 localPosition = content.localPosition;
            Vector3 newPos = localPosition;
            float size = _itemSize.y + _spacing;
            float target = localPosition.y + size-(localPosition.y % size) - _offset - _spacing/2; // This only ever goes down...
            //Todo... add going up aswell...

            if (useAnimationCurve)
            {
                while (curTime < smoothTime)
                {
                    curTime += Time.deltaTime;
                    newPos.y = Mathf.Lerp(content.localPosition.y, target, animationCurve.Evaluate(curTime / smoothTime));
                    content.localPosition = newPos;
                    yield return null;
                }
            }
            else
            {
                while (curTime < smoothTime)
                {
                
                    curTime += Time.deltaTime;
                    newPos.y = Mathf.Lerp(content.localPosition.y, target, curTime / smoothTime);
                    content.localPosition = newPos;
                    yield return null;
                }
            }
           
    
            //This is actually necessary because changing just the position moves the velocity :(
            //yield return new WaitUntil(() => velocity.y < 10);
            velocity = Vector2.zero;
            _isSnapping = false;
            
            //Item at index based on height + 3, wrapped around...
            int itemNum = (int)((content.localPosition.y+_offset) / _itemSize.y + 2) % _numItems;

            //So lazy it's crazy
            //Problems...
            //1) When scrolling, it still has the repeat issue where it jams up :)
            //2) Color all instances of an item... Materials? Do I color them at all? Does it make more sense to just have an item over them
            for(int i = 0; i < _numItems; ++ i)
            {
                content.GetChild(i).GetComponent<Image>().color = Color.black;
                
            }
            
            content.GetChild(itemNum).GetComponent<Image>().color = Color.red;
            
            print("Item selected: " + content.GetChild(itemNum) + ",  " + itemNum);
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
        
        public override void OnDrag(PointerEventData eventData)
        {

            if (eventData.button != PointerEventData.InputButton.Left)
                Debug.Log("Failed 1");

            if (!IsActive())
                Debug.Log("Failed 2");

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                Debug.Log("Failed 3");

            base.OnDrag(eventData);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (isSnappy && !_isSnapping && !_selected  && velocity.sqrMagnitude <= 100) 
                _snap=StartCoroutine(Snap());

            if (content.localPosition.y < -_viewportSize)
            { 
                Vector3 bottom = new Vector3(content.localPosition.x,  content.sizeDelta.y -_viewportSize);
                content.localPosition = bottom;// + offsetY;
                OnEndDrag(new PointerEventData(EventSystem.current));
            }
            else if (content.localPosition.y > content.sizeDelta.y)
            {
                content.localPosition =  Vector3.zero;
                OnEndDrag(new PointerEventData(EventSystem.current));
            }
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!allowUserControl) return;
            Debug.Log("Registered a click");
            AddForce(new Vector2(horizontal?eventData.position.x - content.rect.width / 2 * 1f:0,vertical?eventData.position.y - content.rect.height / 2 * 1f:0));
        }
        #endregion
        
        #if UNITY_EDITOR

        private void WhyNotDeleting()
        {
            if (Application.isPlaying) return;
    
            for (int i = content.childCount - 1; i >= 0; i--)
            {

                GameObject go = content.GetChild(i).gameObject;
                Debug.Log("On destroy: " + go.name);

                if(go.name.ToLower().Contains("clone")) DestroyImmediate(go);
            }
        }
        #endif
    }
}
