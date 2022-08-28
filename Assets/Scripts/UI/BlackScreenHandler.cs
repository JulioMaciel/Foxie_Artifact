using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BlackScreenHandler : MonoBehaviour
    {
        public static BlackScreenHandler Instance;

        Image img;
        
        void Awake() 
        {
            Instance = this;
            img = GetComponent<Image>();
        }
        
        public IEnumerator Darken(float speed = .2f)
        {
            while (img.color.a < 1)
            {
                var c = img.color;
                img.color = new Color(c.r, c.g, c.b, c.a + Time.deltaTime * speed);
                yield return null;
            }
        }
        
        public IEnumerator Lighten(float speed = .2f)
        {
            while (img.color.a < 1)
            {
                var c = img.color;
                img.color = new Color(c.r, c.g, c.b, c.a + Time.deltaTime * speed);
                yield return null;
            }
        }
    }
}