using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class FluturiController : MonoBehaviour
    {
        public Image ButterflyTemplate;
        public int NrButterflies = 10;
        public float MaxScale = 1.0f;
        public float MinScale = 0.5f;
        public float AnimSpeed = 1.0f;
        public float MaxMagnitudeAnimSpeed = 100.0f;
        public float MagnitudeMultiplier = 0.01f;
        public float TurnSpeed = 180.0f;
        public float Spread = 100.0f;
        public float MaxSpeed = 300.0f;
        public float RandomSpeed = 10.0f;
        public float TargetSpeed = 2.0f;
        public float SideSpeed = 100.0f;

        [Header("Resting")]
        public float RestTime = 2.0f;
        public float MinWorkTime = 2.0f;
        public float MaxWorkTime = 6.0f;
        [Header("Color")]
        public Color[] Colors;
        /*
        public float MinHue = 0f;
        public float MaxHue = 1.0f;
        public float MinValue = 0.7f;
        public float MaxValue = 0.9f;
        public float MinSaturation = 0.8f;
        public float MaxSaturation = 1.0f;
        public float Alpha = 0.75f;
        */
        [Header("Attractors")]
        public RectTransform[] Attractors;

        class ButterDesc
        {
            public Transform transform;
            public Vector3 velocity;
            public Vector3 initialScale;
            public float magnitudeSpeedTimer;
            public float restTimer;
        }

        List<ButterDesc> ButterFlies = new List<ButterDesc>();

        MainMenuItem targetItem = null;
        Vector3 lastOffset = Vector3.zero;

        static public FluturiController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetTargetItem(MainMenuItem item)
        {
            targetItem = item;
            if (item != null)
    		{
                lastOffset = item.transform.position;
    		} 
            else
    		{
                lastOffset = Vector3.zero;
    		}
        }

        // Start is called before the first frame update
        void Start()
        {
            UtilsShape.Shuffle(Colors);
            for (int i = 0; i < NrButterflies; i++)
            {
                Image butterFly;
                if (i == 0)
                    butterFly = ButterflyTemplate;
                else
                    butterFly = Instantiate<Image>(ButterflyTemplate, transform);
                //Color color = Random.ColorHSV(MinHue, MaxHue, MinSaturation, MaxSaturation, MinValue, MaxValue);
                //color.a = Alpha;
                //butterFly.color = color;
                butterFly.color = Colors[i % Colors.Length];
                Transform butterTrans = butterFly.transform;
                float scale = Random.Range(MinScale, MaxScale);
                butterTrans.localPosition = Random.insideUnitCircle * Spread;
                butterTrans.localScale = Vector3.one * scale;
                //butterTrans.DOScaleX(0.1f, AnimSpeed * scale).SetEase(Ease.Flash, 2.0f).SetLoops(-1);
                ButterFlies.Add(new ButterDesc() { transform = butterTrans, 
                                                   velocity = Vector3.zero, 
                                                   initialScale = butterTrans.localScale, 
                                                   magnitudeSpeedTimer = 0f,
                                                   restTimer = Random.Range(2, 5) } );
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 updateOffset = Vector3.zero;
            if (targetItem != null)
    		{
                updateOffset = targetItem.transform.position - lastOffset;
                lastOffset = targetItem.transform.position;
    		}

            for (int i = 0; i < ButterFlies.Count; i++)
            {
                ButterDesc butterFly = ButterFlies[i];

                // Timer
                butterFly.restTimer -= Time.deltaTime;
                float restChance = 0.1f;
                if (targetItem != null && butterFly.transform.position.Distance(targetItem.transform.position) < 50.0f)
                    restChance = 0.5f;
                if (butterFly.restTimer < -RestTime || butterFly.restTimer < 0 && Random.value < restChance) // 10% of resting
                    butterFly.restTimer = Random.Range(MinWorkTime, MaxWorkTime);


                // Position and rotation
    			butterFly.transform.position += butterFly.velocity * Time.deltaTime + updateOffset;
                RectTransform rndAttractor = Attractors[Random.Range(0, Attractors.Length)];
                if (targetItem != null)
                    rndAttractor = targetItem.GetComponent<RectTransform>();
                Vector3 rndPos = rndAttractor.position + new Vector3(Random.Range(rndAttractor.rect.xMin, rndAttractor.rect.xMax), Random.Range(rndAttractor.rect.yMin, rndAttractor.rect.yMax));
                if (Input.touchCount > 0)
    			{
                    rndPos = Input.GetTouch(0).position + Random.insideUnitCircle * 50.0f;
    			}
                Vector3 diff = rndPos - butterFly.transform.position;
                if (butterFly.restTimer < 0)
                {
                    butterFly.velocity *= 1 / (1 + (Time.deltaTime * 2.0f));
                }
                else
                {
                    Vector3 newVelocity = butterFly.velocity + (Random.insideUnitCircle.ToVector3() * RandomSpeed + diff.normalized * TargetSpeed) * Time.deltaTime;
                    if (Vector3.Dot(newVelocity.normalized, diff.normalized) < -0.5)
    				{ // Almost opposite, add some side speed
                        newVelocity += (new Vector3(newVelocity.y, -newVelocity.x, 0f)).normalized * SideSpeed * Time.deltaTime;
    				}
                    butterFly.velocity = Vector3.ClampMagnitude(newVelocity, MaxSpeed);
                }
                float rotationZ = Mathf.Atan2(butterFly.velocity.y, butterFly.velocity.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotationZ - 90.0f);
                butterFly.transform.rotation = Quaternion.RotateTowards(butterFly.transform.rotation, targetRotation,
                    (1.0f + Quaternion.Angle(butterFly.transform.rotation, targetRotation) * TurnSpeed) * Time.deltaTime);

    			// Animation
    			Vector3 scale = butterFly.transform.localScale;
    			butterFly.magnitudeSpeedTimer += Mathf.Min(butterFly.velocity.magnitude * MagnitudeMultiplier, MaxMagnitudeAnimSpeed) * Time.deltaTime;
    			float interp = 1.0f - Mathf.Abs(Mathf.Sin(Time.time * butterFly.initialScale.y * AnimSpeed + butterFly.magnitudeSpeedTimer));
    			scale.x = Mathf.Lerp(0.1f, butterFly.initialScale.x, interp);
    			butterFly.transform.localScale = scale;
    		}
        }
    }


}