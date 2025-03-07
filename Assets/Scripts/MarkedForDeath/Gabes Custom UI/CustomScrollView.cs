using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Gabes_Custom_UI
{
    public class CustomScrollView : ScrollRect
    {
        [SerializeField, Tooltip("Insert the background image... This is used to get the width and height")] private RectTransform itemTemplate;

        [SerializeField] private bool infiniteScrollbar;

        [SerializeField] private bool isSnappy;
        [SerializeField] private float smoothTime;

        private Vector2 _itemSize;
        private int _numItems;
        private float _spacing;
        private bool _isSnapping;
        private bool _selected;
        private float _offset;


        protected override void Start()
        {
            onValueChanged.RemoveAllListeners();
            _itemSize = itemTemplate.rect.size;
            /*
            if (content.childCount > 0 && content.GetChild(content.childCount - 1).name.Contains("(Clone)"))
            {

                if (infiniteScrollbar)
                {
                    BindScrollBar();
                }

                if (isSnappy)
                {
                    HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup =
                        content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    spacing = horizontalOrVerticalLayoutGroup.spacing;
                    offset = horizontalOrVerticalLayoutGroup.padding.top;
                    HandleSnaps();
                }

                return;
            } */
           

            _numItems = content.childCount;
            Vector2 size = content.sizeDelta;
            content.localPosition = new Vector3(0, 0);

            if (content.TryGetComponent(out VerticalLayoutGroup vlg))
            {

                if (horizontal) size.x = _itemSize.x * _numItems + vlg.padding.horizontal;
                if (vertical) size.y = _itemSize.y * _numItems + vlg.spacing * (_numItems - 1) + vlg.padding.vertical;
            }
            else if (content.TryGetComponent(out HorizontalLayoutGroup hlg))
            {
                if (horizontal) size.x = _itemSize.x * _numItems + hlg.spacing * (_numItems - 1) + hlg.padding.horizontal;
                if (vertical) size.y = _itemSize.y * _numItems + hlg.padding.vertical;
            }

            else
            {
                Debug.LogError("Content not automatically sized as the object: " + content.name +
                               " contains no layout group");
            }
            int visible = 0;
            content.sizeDelta = size;
            if (vertical)
            {
                visible = Mathf.CeilToInt(viewRect.rect.height / (_itemSize.y + vlg.padding.top));
                //We need to factor spacing, yet it's only the spacing of the object that's actually visible.
                visible = Mathf.CeilToInt(viewRect.rect.height /
                                          (_itemSize.y + vlg.spacing * (visible - 1) + vlg.padding.top)) +
                          1; // and one?
                    
                size.y += viewRect.rect.height;
            }
            else
            {
                visible = Mathf.CeilToInt(viewRect.rect.width / (_itemSize.x + vlg.padding.left));
                //We need to factor spacing, yet it's only the spacing of the object that's actually visible.
                visible = Mathf.CeilToInt(viewRect.rect.width /
                                          (_itemSize.x + vlg.spacing * (visible - 1) + vlg.padding.left)) +
                          1; // and one?
                size.x += viewRect.rect.width;

            }
            /*
            if (numItems < visible)
            {
                infiniteScrollbar = false;
                vertical = false;
                horizontal = false;
                isSnappy = false;
                Debug.LogWarning("Cannot infinitely scroll.");
                return;
            }*/
            
            if (isSnappy)
            {
                HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup =
                    content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                _spacing = horizontalOrVerticalLayoutGroup.spacing;
                _offset = horizontalOrVerticalLayoutGroup.padding.top;
            }

            if (infiniteScrollbar)
            {
                //If the scroll bar is infinite... Then we should duplicate however many items are visible on the screen by default... Then we just loop back around...

                //First determine how many objects are visible by default...
                //Based on the viewport height... and the object size, spacing and padding...
                

                

                #if UNITY_EDITOR
                if (!content.GetChild(_numItems-1).name.Contains("(Clone)"))
                {
                    print("No Clones detected, not adding more...");
                    for (int i = 0; i < visible; ++i)
                    {
                        Instantiate(content.GetChild(i), content);
                    }
                    content.sizeDelta = size;
                }
                #else
                    for (int i = 0; i < visible; ++i)
                    {
                        Instantiate(content.GetChild(i), content);
                    }
                content.sizeDelta = size;
                #endif
                _numItems = content.childCount;


                print($"Duplicating {visible} elements for infinite scrolling");
                BindScrollBar();
                

            }

            

            print($"There are {_numItems} items, occupying a size of: {content.rect.size}, based on the item size of ({_itemSize}) * {_numItems}");
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
            
            while (curTime < smoothTime)
            {
                
                curTime += Time.deltaTime;
                newPos.y = Mathf.Lerp(content.localPosition.y, target, curTime / smoothTime);
                content.localPosition = newPos;
                yield return null;
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
            StopAllCoroutines();
            _isSnapping = false;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            _selected = false;
        }

        private void Update()
        {
            if (!isSnappy || _isSnapping || _selected || velocity.sqrMagnitude > 100) return;
            StartCoroutine(Snap());

        }

        private void BindScrollBar()
        {
            Vector3 originPos = content.position;
            if (vertical)
            {
                float h = viewport.rect.height;
                Vector3 topside = new Vector3(0, content.rect.height -h, 0);
                onValueChanged.AddListener(val =>
                {
                    //print((val.y * content.sizeDelta.y)+" <= " + (viewport.rect.height));
                    if (val.y < 0)
                    {
                        content.position = originPos;// + offsetY;
                    }
                    else if (val.y > 1)
                    {
                        content.localPosition = topside;
                    }
                
                    velocity = new Vector2(0,velocity.y);
                });
            }
            else
            {
                float w = viewport.rect.width;
                Vector3 topside = new Vector3(content.rect.width -w, 0, 0);
                onValueChanged.AddListener(val =>
                {
                    //print((val.y * content.sizeDelta.y)+" <= " + (viewport.rect.height));
                    if (val.x < 0)
                    {
                        content.position = originPos;// + offsetY;
                    }
                    else if (val.x > 1)
                    {
                        content.localPosition = topside;
                    }
                
                    velocity = new Vector2(velocity.x,0);
                });
            }
        }


        public void AddForce(float amount)
        {
            print("adding force: " + amount);
            velocity += new Vector2(amount, amount);
            _isSnapping = false;
            _selected = false;
            StopAllCoroutines();
        }

        public void AddRandomForce(float amount)
        {
            AddForce(Random.Range(amount / 10, amount) * (Random.Range(0, 2) == 1 ? -1 : 1));
        }
    }
}
