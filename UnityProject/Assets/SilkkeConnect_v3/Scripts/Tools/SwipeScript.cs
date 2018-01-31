using UnityEngine;

namespace Silkke
{
    public class SwipeScript : MonoBehaviour
    {
        private float   fingerStartTime = 0.0f;
        private Vector2 fingerStartPos  = Vector2.zero;

        private bool    isSwipe         = false;
        private float   minSwipeDist    = 50.0f;
        private float   maxSwipeTime    = 0.5f;

        // Screen Touch handler function for every device who support it
        void Update()
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            isSwipe = true;
                            fingerStartTime = Time.time;
                            fingerStartPos = touch.position;
                            break;

                        case TouchPhase.Canceled:
                            isSwipe = false;
                            break;

                        case TouchPhase.Ended:

                            float gestureTime = Time.time - fingerStartTime;
                            float gestureDist = (touch.position - fingerStartPos).magnitude;

                            if (isSwipe && gestureTime < maxSwipeTime && gestureDist > minSwipeDist)
                            {
                                Vector2 direction = touch.position - fingerStartPos;
                                Vector2 swipeType = Vector2.zero;

                                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                                    swipeType = Vector2.right * Mathf.Sign(direction.x);
                                else
                                    swipeType = Vector2.up * Mathf.Sign(direction.y);

                                if (swipeType.x != 0.0f)
                                {

                                }

                                if (swipeType.y != 0.0f)
                                {
                                    if (swipeType.y > 0.0f)
                                    {
                                        // MOVE UP
                                    }
                                    else
                                    {
                                        // MOVE DOWN
                                    }
                                }

                            }
                            break;
                    }
                }
            }
        }
    }
}