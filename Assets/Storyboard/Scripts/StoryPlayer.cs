using UnityEngine;
using UnityEngine.UI;

namespace VMail
{
    public class StoryPlayer : MonoBehaviour
    {
        public enum PlaybackMode { SliderFree, SliderPage, Forward, ForwardTil, Backward, BackwardTil, None };

        [SerializeField]
        private StoryEditor storyEditor;

        [Space(10)]
        [SerializeField]
        private float playbackSpeed = 0.4f;
        [SerializeField]
        [Range(0f, 2f)]
        private float pagePauseTime = 1f;

        [Space(10)]
        [SerializeField]
        private Button forwardBtn;
        [SerializeField]
        private Button forwardTilBtn;

        [Space(10)]
        [SerializeField]
        private Button backwardBtn;
        [SerializeField]
        private Button backwardTilBtn;

        [Space(10)]
        [SerializeField]
        private Slider slider;

        private float prevSldierValue;
        private PlaybackMode currMode = PlaybackMode.None;
        private float pagePauseTimer;


        private void Awake()
        {
            this.prevSldierValue = this.slider.value;
        }

        public void PausePlaying()
        {
            this.currMode = PlaybackMode.None;

            this.UpdateButtonTexts();
        }

        public void ProcessButtonClick(Button clicked)
        {
            if (clicked == this.forwardBtn)
            {
                this.pagePauseTimer = 0f;
                this.currMode = this.currMode == PlaybackMode.Forward ? PlaybackMode.None : PlaybackMode.Forward;
            }
            else if (clicked == this.forwardTilBtn)
            {
                if (this.currMode == PlaybackMode.None)
                {
                    float prog = this.GetTransitionValue() - (int)this.GetTransitionValue();
                    if (prog >= 0.9999f)
                    {
                        // move to the next transition, e.g. 0.9999 -> 1.0
                        this.SetTransitionValue((int)this.GetTransitionValue() + 1f);
                    }
                }

                this.currMode = this.currMode == PlaybackMode.ForwardTil ? PlaybackMode.None : PlaybackMode.ForwardTil;
            }
            else if (clicked == this.backwardBtn)
            {
                this.pagePauseTimer = 0f;
                this.currMode = this.currMode == PlaybackMode.Backward ? PlaybackMode.None : PlaybackMode.Backward;
            }
            else if (clicked == this.backwardTilBtn)
            {
                if (this.currMode == PlaybackMode.None)
                {
                    float prog = this.GetTransitionValue() - (int)this.GetTransitionValue();
                    if (prog <= 0)
                    {
                        // move to the previous transition, e.g. 1.0 -> 0.9999
                        this.SetTransitionValue(((int)this.GetTransitionValue()) - 1f + 0.9999f);
                    }
                }

                this.currMode = this.currMode == PlaybackMode.BackwardTil ? PlaybackMode.None : PlaybackMode.BackwardTil;
            }

            this.UpdateButtonTexts();
        }

        // process the slider click 
        public void ProcessSliderClick()
        {
            this.currMode = PlaybackMode.SliderPage;

            this.UpdateButtonTexts();
        }

        public void OnSliderValueChanged()
        {
            if (this.currMode == PlaybackMode.SliderFree || this.currMode == PlaybackMode.SliderPage)
            {
                if (this.currMode == PlaybackMode.SliderPage)
                {
                    int idxPrev = (int)this.prevSldierValue;
                    int idxCurr = (int)this.GetTransitionValue();

                    if (!Input.GetMouseButtonDown(0) && idxPrev < idxCurr) // trying to move to the next page
                    {
                        //Debug.Log("slider next " + idxPrev + " " + idxCurr);
                        this.prevSldierValue = (int)this.prevSldierValue + 0.9999f;
                        this.slider.value = (int)this.prevSldierValue + 0.9999f;
                    }
                    else if (!Input.GetMouseButtonDown(0) && idxPrev > idxCurr) // tyring to move to the previous page
                    {
                        //Debug.Log("slider prev " + idxPrev + " " + idxCurr);
                        this.prevSldierValue = (int)this.prevSldierValue;
                        this.slider.value = (int)this.prevSldierValue;
                    }
                    else
                    {
                        //Debug.Log("slider curr " + idxPrev + " " + idxCurr);
                        this.prevSldierValue = this.slider.value;
                        this.storyEditor.UpdateStateFromCurrentTransition();
                    }
                }
                else
                {
                    this.prevSldierValue = this.slider.value;
                    this.storyEditor.UpdateStateFromCurrentTransition();
                }
            }
            else if (this.currMode == PlaybackMode.None)
            {
                this.prevSldierValue = this.slider.value;
                this.storyEditor.UpdateStateFromCurrentTransition();
            }
            else
            {
                this.storyEditor.UpdateStateFromCurrentTransition();
            }
        }

        // update the button texts depending on the playback mode
        private void UpdateButtonTexts()
        {
            this.forwardBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.Forward ? "||" : ">";
            this.forwardTilBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.ForwardTil ? "||" : ">|";

            this.backwardBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.Backward ? "||" : "<";
            this.backwardTilBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.BackwardTil ? "||" : "|<";
        }

        public void Clear()
        {
            this.SetTransitionSize(0f, 0f);
        }

        public void InvokeSliderOnValueChanged()
        {
            this.slider.onValueChanged.Invoke(this.slider.value);
        }


        public void GoToPreviousPage()
        {
            this.PausePlaying();

            float prog = this.GetTransitionValue() - (int)this.GetTransitionValue();
            if (prog <= 0f)
            {
                this.SetTransitionValue((int)(this.GetTransitionValue() - 1));
            }
            else
            {
                this.SetTransitionValue((int)this.GetTransitionValue());
            }
        }

        public void GoToNextPage()
        {
            this.PausePlaying();

            this.SetTransitionValue(((int)this.GetTransitionValue()) + 1);
        }

        public void GoToLastPage()
        {
            this.PausePlaying();

            this.SetTransitionValue(this.slider.maxValue);
        }

        public void GoToFirstPage()
        {
            this.PausePlaying();

            this.SetTransitionValue(0);
        }

        public void SetTransitionSize(float min, float max)
        {
            if (max < min)
            {
                //Debug.LogWarning("invalid range... " + min + "~" + max);
                return;
            }

            this.slider.minValue = min;
            this.slider.maxValue = max;
        }

        public void SetTransitionValue(float val)
        {
            this.prevSldierValue = val;
            this.slider.value = val;
        }

        public void SetTransitionValueAsRatio(float ratio)
        {
            this.SetTransitionValue((this.slider.maxValue - this.slider.minValue) * ratio + this.slider.minValue);
        }

        public void SetInteractable(bool interactable)
        {
            this.slider.interactable = interactable;
        }

        public float GetTransitionValue()
        {
            return this.slider.value;
        }

        public float GetMinValue()
        {
            return this.slider.minValue;
        }

        public float GetMaxValue()
        {
            return this.slider.maxValue;
        }

        private void Update()
        {
            // slider
            if (this.currMode == PlaybackMode.SliderFree || this.currMode == PlaybackMode.SliderPage)
            {
                this.currMode = Input.GetKey(KeyCode.LeftShift) ? PlaybackMode.SliderFree : PlaybackMode.SliderPage;

                this.UpdateButtonTexts();
            }
            // animation...
            else if (this.currMode == PlaybackMode.Backward || this.currMode == PlaybackMode.BackwardTil ||
                this.currMode == PlaybackMode.Forward || this.currMode == PlaybackMode.ForwardTil)
            {
                // determine the new slider value
                float speed = (this.currMode == PlaybackMode.Backward || this.currMode == PlaybackMode.BackwardTil) ? this.playbackSpeed * -1f : this.playbackSpeed;
                float val = this.GetTransitionValue() + (Time.deltaTime * speed);

                // handles boundary cases
                if (val < this.slider.minValue)
                {
                    val = this.slider.minValue;
                    this.PausePlaying();
                }
                else if (val > this.slider.maxValue)
                {
                    val = this.slider.maxValue;
                    this.PausePlaying();
                }
                // pause if reached the next page
                else
                {
                    int idxPrev = (int)this.prevSldierValue;
                    int idxCurr = (int)val;

                    // reached the next page
                    if (this.currMode == PlaybackMode.ForwardTil && idxPrev < idxCurr)
                    {
                        val = idxPrev + 0.9999f;
                        this.PausePlaying();
                    }
                    // reached the previous page
                    else if (this.currMode == PlaybackMode.BackwardTil && idxPrev > idxCurr)
                    {
                        val = idxPrev;
                        this.PausePlaying();
                    }
                    // reached the next page
                    if (this.currMode == PlaybackMode.Forward && idxPrev < idxCurr)
                    {
                        this.pagePauseTimer += Time.deltaTime;
                        if (this.pagePauseTimer > this.pagePauseTime) // move to the next page
                        {
                            // move to the next transition, e.g. 0.9999 -> 1.0
                            this.SetTransitionValue((int)this.GetTransitionValue() + 1f);
                            this.pagePauseTimer = 0f;
                        }
                        else // stay put and increase the page timer
                        {
                            val = idxPrev + 0.9999f;
                        }
                    }
                    // reached the previous page
                    else if (this.currMode == PlaybackMode.Backward && idxPrev > idxCurr)
                    {
                        this.pagePauseTimer += Time.deltaTime;
                        if (this.pagePauseTimer > this.pagePauseTime) // move to the previous page
                        {
                            // move to the previous transition, e.g. 1.0 -> 0.9999
                            this.SetTransitionValue(((int)this.GetTransitionValue()) - 1f + 0.9999f);
                            this.pagePauseTimer = 0f;
                        }
                        else // stay put and increase the page timer
                        {
                            val = idxPrev;
                        }
                    }
                }

                // update the transition value
                this.prevSldierValue = this.slider.value;
                this.slider.value = val;
            }
            else
            {
                this.prevSldierValue = this.slider.value;
            }
        }

    }
}