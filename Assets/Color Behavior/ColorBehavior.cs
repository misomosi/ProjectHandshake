/* Color Behavior
 * Gear5 Media LLC (Gear5Media.com)
 * Brendan Dickinson
 * 6/25/2015
 * 
 * This script is meant to allow easy customization of colors, tints, and their behaviors 
 * on any object or group of objects.
 * You can specify the material and/or shader that will be used as well as several options
 * for coloring and color behavior of your object.
 * 
 * Updates:
 * 9/12/15
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColorBehavior : MonoBehaviour
{
    //Color value (using Unity's built in color inspector)
    [Header("Color")]
    [SerializeField]
    private Color color;
    //RGBA slider values RGB going from 0-255, while Alpha goes from 0-1
    [Range(0, 255), SerializeField]
    private float r;
    [Range(0, 255), SerializeField]
    private float g;
    [Range(0, 255), SerializeField]
    private float b;
    [Range(0.0f, 1), SerializeField]
    private float a;
    //Hex value is the color in hexidecimal
    [SerializeField]
    private string hexValue;
    //Hue, Saturation, and Light/Value variables
    [Range(-1, 5), SerializeField]
    private float h;
    [Range(0, 1), SerializeField]
    private float s;
    [Range(0, 1), SerializeField]
    private float l;

    //Used to give the same behaviour to a group of objects
    [Header("Grouping")]
    [SerializeField]
    private bool group;
    [SerializeField]
    private List<GameObject> groupList = new List<GameObject>();

    //Interval measures seconds between color changes (used for blink and cycle)
    [Header("Speed")]
    [SerializeField]
    private float interval;

    //Booleans to designate active behavior
    [Header("Behavior")]
    [SerializeField]
    private bool random = false;
    [SerializeField]
    private bool easing = false;
    [SerializeField]
    private bool useGrayscale = false;
    [SerializeField]
    private bool blink = false;
    [SerializeField]
    private bool cycle = false;

    //List of colors to cycle through when cycle is the designated behavior
    [SerializeField]
    private List<Color> cycleColors = new List<Color>();

    //Shader and material properties of the object
    [Header("Shader & Material")]
    [SerializeField]
    private Shader shader;
    [SerializeField]
    private Material material;
    [SerializeField]
    private string shaderProperty = "";
    [SerializeField]
    private List<string> shaderProperties;

    //Variables used for changing from one behavior to another or updating color
    private Color previousColor;
    private Color previousRGBA;
    private Color currentRGBA;
    private Color originalColor;
    private Color targetColor;
    private Color lerpColor;
    private Color changedColor;
    private HSL previousHSL;
    private HSL currentHSL;
    private string previousHex;
    private int cycleCounter;
    private int previousCounter;
    private float r2;
    private float g2;
    private float b2;
    private float count = 0;
    private float easeTime = 0;
    private bool needRandom = false;
    private bool cycleInitialized = false;
    private bool needNextColor = true;
    private bool hasRenderer = true;
	
	//Cache for the renderer of object
	private Renderer rendererCache;

    //Set all initial values
    void Start()
    {
        //default color options in inspector
        previousColor = new Color(0, 0, 0, 0);
        previousRGBA = new Color(0, 0, 0, 0);
        previousHSL = new HSL();
        previousHex = "";

        //Start cycleCounter at 0
        cycleCounter = 0;

        //Set the r2, g2, b2, to initial random values
        NewRandom();

        //If shader property is not set in inspector use _Color
        if (shaderProperty == "")
            shaderProperty = "_Color";

        //Cache renderer and set the material and shader
        if (group)
        {
            //if group list is empty fill it with all the direct children of this gameObject
            if (groupList.Count == 0)   //You can designate your own group of object that aren't under a single parent, otherwise this happens
            {
                foreach (Transform child in gameObject.transform)
                {
                    groupList.Add(child.gameObject);
                }
            }
            hasRenderer = false;
            //set each object in the group with designated material and shader
            for (int i = 0; i < groupList.Count; i++)
            {
                if (groupList[i].GetComponent<Renderer>() == null)
                {
                    Debug.Log(groupList[i].name + " does not have a renderer and so it was removed from the list.");
                    groupList.Remove(groupList[i]);
                }
                else
                {
                    rendererCache = groupList[i].GetComponent<Renderer>();
                    SetMaterialAndShader(rendererCache);
                    hasRenderer = true;
                }
            }
        }
        else
        {
            //Cache this objects renderer and set the designated material and shader
            if (gameObject.GetComponent<Renderer>() != null)
            {
                rendererCache = gameObject.GetComponent<Renderer>();
                SetMaterialAndShader(rendererCache);
            }
            else
            {
                Debug.Log(gameObject.name + " does not have a renderer and so this script cannot be used.");
                hasRenderer = false;
            }
        }

        if (hasRenderer)
        {
            //Using the current renderer find all the properties that are of type color and add them to the Shader Properties list in the inspector
            for (int i = 0; i < UnityEditor.ShaderUtil.GetPropertyCount(rendererCache.material.shader); i++)
            {
                if (UnityEditor.ShaderUtil.GetPropertyType(rendererCache.material.shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.Color)
                    shaderProperties.Add(UnityEditor.ShaderUtil.GetPropertyName(rendererCache.material.shader, i));
            }

            //Find the initial color and set all other instpector values accordingly
            UpdateColorValues(FindChangedColor());
        }
    }

    //Use Update for when we are not easing
    void Update()
    {
        if (!easing && hasRenderer)
        {
            if (group)
            {
                for (int i = 0; i < groupList.Count; i++)
                {
                    rendererCache = groupList[i].GetComponent<Renderer>();
                    if (i == 0)
                    {
                        UpdateRenderer();
                        if (rendererCache.material.HasProperty(shaderProperty))
                            UpdateColorValues(rendererCache.material.GetColor(shaderProperty));
                        else
                            Debug.Log(groupList[i].gameObject.name + " in your group doesn't have the correct shader property");
                    }
                    else
                    {
                        if (rendererCache.material.HasProperty(shaderProperty))
                            rendererCache.material.SetColor(shaderProperty, color);
                        else
                            Debug.Log(groupList[i].gameObject.name + " in your group doesn't have the correct shader property");
                    }
                }
            }
            else
            {
                UpdateRenderer();
            }
            count += Time.deltaTime;
        }
    }

    //FixedUpdate to work with Lerping using physics engine (consistent frame rate)
    void FixedUpdate()
    {
        if (easing && hasRenderer)
        {
            //when calculating easeTime we divide by this so we don't want it to be 0
            if (interval == 0)
                interval += .00001f;

            if (group)
            {
                for (int i = 0; i < groupList.Count; i++)
                {
                    rendererCache = groupList[i].GetComponent<Renderer>();
                    if (i == 0) //only do this once per iteration through all objects in the group
                    {
                        if (rendererCache.material.HasProperty(shaderProperty))
                        {
                            DoColorBehavior(rendererCache);
                            color = rendererCache.material.GetColor(shaderProperty);
                        }
                        else
                            Debug.Log(groupList[i].gameObject.name + " in your group doesn't have the correct shader property");
                    }
                    if (rendererCache.material.HasProperty(shaderProperty))
                    {
                        rendererCache.material.SetColor(shaderProperty, lerpColor);
                    }
                    else
                        Debug.Log(groupList[i].gameObject.name + " in your group doesn't have the correct shader property");
                }
            }
            else  //if not grouping we can simply update the cachedRenderer
            {
                UpdateRenderer();
            }
            if (easeTime < 1)
                easeTime += Time.deltaTime / interval;  //increment easeTime so that it reaches 1 after interval # of seconds
        }
    }

    //Sets new random RGB values for the object
    private void NewRandom()
    {
        r2 = Random.Range(0.0f, 1.0f);
        g2 = Random.Range(0.0f, 1.0f);
        b2 = Random.Range(0.0f, 1.0f);
        needRandom = false;
    }

    //Takes the current rgb values and finds the corresponding HSL values, then returns the HSL object
    private HSL UpdateHSL()
    {
        float _R = r / 255.0f;
        float _G = g / 255.0f;
        float _B = b / 255.0f;

        float _Min = Mathf.Min(Mathf.Min(_R, _G), _B);
        float _Max = Mathf.Max(Mathf.Max(_R, _G), _B);
        float _Delta = _Max - _Min;

        float localH = 0;
        float localS = 0;
        float localL = (float)((_Max + _Min) / 2.0f);

        if (_Delta != 0)
        {
            if (localL < 0.5f)
            {
                localS = (float)(_Delta / (_Max + _Min));
            }
            else
            {
                localS = (float)(_Delta / (2.0f - _Max - _Min));
            }

            if (_R == _Max)
            {
                localH = (_G - _B) / _Delta;
            }
            else if (_G == _Max)
            {
                localH = 2f + (_B - _R) / _Delta;
            }
            else if (_B == _Max)
            {
                localH = 4f + (_R - _G) / _Delta;
            }
        }

        return new HSL(localH, localS, localL);
    }

    //Takes the given HSL object and returns the corresponding RGBA color
    private Color HSLToRGB(HSL newHSL)
    {
        float localR;
        float localG;
        float localB;

        if (newHSL.saturation == 0)
        {
            localR = newHSL.light;
            localG = newHSL.light;
            localB = newHSL.light;
        }
        else
        {
            float t1, t2;
            float th = newHSL.hue / 6.0f;

            if (newHSL.light < 0.5f)
            {
                t2 = newHSL.light * (1 + newHSL.saturation);
            }
            else
            {
                t2 = (newHSL.light + newHSL.saturation) - (newHSL.light * newHSL.saturation);
            }
            t1 = 2 * newHSL.light - t2;

            float tr, tg, tb;
            tr = th + (1.0f / 3.0f);
            tg = th;
            tb = th - (1.0f / 3.0f);

            tr = ColorCalc(tr, t1, t2);
            tg = ColorCalc(tg, t1, t2);
            tb = ColorCalc(tb, t1, t2);
            localR = tr;
            localG = tg;
            localB = tb;
        }

        return new Color(localR, localG, localB, 1);
    }

    //Used by HSLToRGB to find certain values
    private static float ColorCalc(float c, float t1, float t2)
    {
        if (c < 0) c += 1f;
        if (c > 1) c -= 1f;
        if (6.0 * c < 1.0) return t1 + (t2 - t1) * 6.0f * c;
        if (2.0 * c < 1.0) return t2;
        if (3.0 * c < 2.0) return t1 + (t2 - t1) * (2.0f / 3.0f - c) * 6.0f;
        return t1;
    }

    //converts a hexidecimal color value to RGBA and returns it as Color
    private Color HexToColor(string hexValue)
    {
        float localR;
        float localG;
        float localB;
        string subString;
        try
        {
            subString = hexValue.Substring(0, 2);
            localR = System.Convert.ToInt32(subString, 16);
            subString = hexValue.Substring(2, 2);
            localG = System.Convert.ToInt32(subString, 16);
            subString = hexValue.Substring(4, 2);
            localB = System.Convert.ToInt32(subString, 16);
            Debug.Log("Finishing Hex Update");
            return new Color(localR / 255.0f, localG / 255.0f, localB / 255.0f, 1);
        }
        catch (System.FormatException e)
        {
            Debug.Log("<color=maroon><size=15>THAT'S NOT A HEXIDECIMAL!</size></color>");
            Debug.Log(e);
            return new Color(0, 0, 0, 0);
        }
    }

    //-if no shader or material is designated in the inspector and there's no material on the object
    //then it will set the material to default with transparent/diffuse type shader on it
    //-if the object has both a material and shader designated it will first set the material, then
    //the specified shader (this means the shader on the designated material will be ignored
    //-if only the material or shader is designated they will be set to the object respectively
    private void SetMaterialAndShader(Renderer rendererCache)
    {
        if (shader == null && material == null)
        {
            if (rendererCache.material == null)
            {
                rendererCache.material = new Material(Shader.Find("Transparent/Diffuse"));
            }
        }
        else if (material != null && shader != null)
        {
            rendererCache.material = material;
            rendererCache.material.shader = shader;
        }
        else if (material != null)
        {
            rendererCache.material = material;
        }
        else if (shader != null)
        {
            rendererCache.material.shader = shader;
        }
    }

    //sets the color to the hex value designated in inspector
    private void SetHexValue(Renderer rendererCache)
    {
        Color hexColor;
        previousHex = hexValue;
        hexColor = HexToColor(hexValue);
        rendererCache.material.SetColor(shaderProperty, new Color(hexColor.r / 255.0f, hexColor.g / 255.0f, hexColor.b / 255.0f, hexColor.a));
    }

    //updates renderer to color in inspector
    private void UpdateColor(Renderer rendererCache, Color color)
    {
        rendererCache.material.SetColor(shaderProperty, color);
        UpdateColor(color);
    }

    //updates renderer to color in inspector
    private void UpdateColor(Color color)
    {
        r = (color.r * 255.0f);
        g = (color.g * 255.0f);
        b = (color.b * 255.0f);
        a = (float)(color.a);
    }

    //live updates hex value as rgb value changes
    private string UpdateHex()
    {
        int rToInt = (int)r;
        int gToInt = (int)g;
        int bToInt = (int)b;

        string hexString;
        hexString = rToInt.ToString("X2") + gToInt.ToString("X2") + bToInt.ToString("X2");
        return hexString;
    }

    //cycle to the next color in the cycleList
    private void CycleColor(Renderer rendererCache)
    {
        if (cycleCounter == cycleColors.Count)
            cycleCounter = 0;
        //if random is also selected cycle through cycleColors list randomly
        //else cycle linearly from beginning to end of the list.
        if (random)
        {
            previousCounter = cycleCounter;
            //finds a new color in the list
            while ((cycleCounter = Random.Range(0, cycleColors.Count)) == previousCounter && cycleColors.Count > 1) { }
            rendererCache.material.SetColor(shaderProperty, cycleColors[cycleCounter]);
        }
        else
        {
            rendererCache.material.SetColor(shaderProperty, cycleColors[cycleCounter]);
            cycleCounter++;
        }
        //if cycle is true blink should not be true & vice versa
        blink = false;
        count = 0;
    }

    //continually ease to the next color in the cycleList
    private void EasingCycleColor(Renderer rendererCache)
    {
        //if at the end of the list wrap back around
        if (cycleCounter == cycleColors.Count)
            cycleCounter = 0;

        InitializeCycleEasing(rendererCache);
        targetColor = cycleColors[cycleCounter];
        LerpColor();
        //if you've eased to the target color find next color in the cycle
        if (lerpColor == targetColor)
            SetNextLerpColor();
    }

    //ease the blink to the next random color
    private void EasingBlinkColor(Renderer rendererCache)
    {
        InitializeBlinkEasing(rendererCache);
        LerpColor();
        //if random is also selected cycle through cycleColors list randomly
        //else cycle linearly from beginning to end of the list.
        if (lerpColor == targetColor)
            SetNextLerpColor();
    }

    private void InitializeCycleEasing(Renderer rendererCache)
    {
        //set initial values
        if (!cycleInitialized)
        {
            originalColor = rendererCache.material.GetColor(shaderProperty);
            targetColor = cycleColors[cycleCounter];
            cycleInitialized = true;
        }
        //if cycle is true blink should not be true & vice versa
        blink = false;
    }

    private void InitializeBlinkEasing(Renderer rendererCache)
    {
        //set up initial values
        if (needNextColor)
        {
            originalColor = rendererCache.material.GetColor(shaderProperty);
            targetColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), a);

            //if blink is true cycle and random should be false
            random = false;
            cycle = false;
            needRandom = false;
            needNextColor = false;
        }
    }

    private void LerpColor()
    {
        if (easeTime >= 1)  //if time is up set color to target color
            lerpColor = targetColor;
        else
            lerpColor = Color.Lerp(originalColor, targetColor, easeTime);   //slightly ease color

        if (!group)
        {
            //set renderer to new color
            rendererCache.material.SetColor(shaderProperty, lerpColor);
        }
        UpdateColorValues(rendererCache.material.GetColor(shaderProperty)); //update all color values in inspector
    }

    private void SetNextLerpColor()
    {
        originalColor = targetColor;
        if (blink)
        {
            targetColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), a);
        }
        else if (cycle)
        {
            //if random is also selected pick next color in cycleList randomly
            //else cycle linearly from beginning to end of the list.
            if (random)
            {
                previousCounter = cycleCounter;
                //finds a new color in the list
                while ((cycleCounter = Random.Range(0, cycleColors.Count)) == previousCounter && cycleColors.Count > 1) { }
            }
            else
                cycleCounter++;
        }
        easeTime = 0;
    }

    //blink to random color at interval speed
    private void BlinkColor(Renderer rendererCache)
    {
        //if blink is true cycle and random should be false
        random = false;
        cycle = false;
        rendererCache.material.SetColor(shaderProperty, new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), a));
        count = 0;
        needRandom = false;
    }

    //set color to a random rgb value that is generated
    private void SetRandomColor(Renderer rendererCache)
    {
        if (needRandom == true)
        {
            NewRandom();
        }
        blink = false;
        rendererCache.material.SetColor(shaderProperty, new Color(r2, g2, b2, a));
        UpdateColorValues(rendererCache.material.GetColor(shaderProperty));
    }

    //Determine what behavior to do and call that function
    private void DoColorBehavior(Renderer rendererCache)
    {
        if (!random)
        {
            needRandom = true;
        }
        //if cycle is on, cycle through the cycleColors list if it's not empty
        if (cycle && count >= interval && cycleColors.Count > 0 || easing && cycle && cycleColors.Count > 0)
        {
            if (!easing)
                CycleColor(rendererCache);
            else
                EasingCycleColor(rendererCache);
        }
        //if blink is true blink through random colors at the designated interval speed (seconds)
        else if (blink && count >= interval || easing && blink)
        {
            if (!easing)
                BlinkColor(rendererCache);
            else
                EasingBlinkColor(rendererCache);
            cycleInitialized = false;
        }
        //if random is true and cycle is not, find a random color and set it to the object (static)
        else if (random && !cycle)
        {
            SetRandomColor(rendererCache);
            cycleInitialized = false;
        }
        //if none of these are true (Random, Blink, Cycle) then set the object to the designated RGBA values in the inspector
        else if (!blink && !cycle)
        {
            UpdateColorValues(FindChangedColor());
            rendererCache.material.SetColor(shaderProperty, color);
            cycleInitialized = false;
        }
        
    }

    //updates all inspector values to the most recently changed color
    private void UpdateColorValues(Color newColor)
    {
        color = newColor;
        r = (newColor.r * 255.0f);
        g = (newColor.g * 255.0f);
        b = (newColor.b * 255.0f);
        a = newColor.a;
        currentRGBA = newColor;
        currentHSL = UpdateHSL();
        h = currentHSL.hue;
        s = currentHSL.saturation;
        l = currentHSL.light;
        hexValue = UpdateHex();

        previousColor = color;
        previousRGBA = currentRGBA;
        previousHSL = currentHSL;
        previousHex = hexValue;
    }

    //if shader has designated color property update colors and do the behaviors
    private void UpdateRenderer()
    {
        if (rendererCache.material.HasProperty(shaderProperty))
        {
            //live update color
            if (AreNotSameColor(rendererCache.material.GetColor(shaderProperty), color))
            {
                UpdateColorValues(FindChangedColor());
                UpdateColor(rendererCache, color);
            }
            else
            {
                DoColorBehavior(rendererCache);
                color = rendererCache.material.GetColor(shaderProperty);
            }
            UpdateColorValues(color);
        }
        else
            Debug.Log(rendererCache.gameObject.name + " Doesn't have the correct Shader Property!");
    }

    //finds the most recently changed color value
    private Color FindChangedColor()
    {
        currentRGBA = new Color(r / 255f, g / 255f, b / 255f, a);
        currentHSL = new HSL(h, s, l);

        if (AreNotSameColor(color, previousColor))
        {
            changedColor = color;
            previousColor = color;
        }
        else if (currentRGBA != previousRGBA)
        {
            changedColor = currentRGBA;
            previousRGBA = currentRGBA;
        }
        else if (!currentHSL.Equals(previousHSL))
        {
            changedColor = HSLToRGB(currentHSL);
            previousHSL.hue = currentHSL.hue;
            previousHSL.saturation = currentHSL.saturation;
            previousHSL.light = currentHSL.light;
        }
        else if (hexValue.Length == 6 && hexValue != previousHex)
        {
            changedColor = HexToColor(hexValue);
            previousHex = hexValue;
        }

        if (useGrayscale)
        {
            changedColor = new Color(changedColor.grayscale, changedColor.grayscale, changedColor.grayscale);
        }
        return changedColor;
    }

    //compares two colors and returns true if they are at all different
    public bool AreNotSameColor(Color color1, Color color2)
    {
        //rounds numbers so they don't have to be exactly the same but just close enough
        if ((float)System.Math.Round((color1.r / 255f), 2) != (float)System.Math.Round((color2.r / 255f), 2) ||
            (float)System.Math.Round((color1.g / 255f), 2) != (float)System.Math.Round((color2.g / 255f), 2) ||
            (float)System.Math.Round((color1.b / 255f), 2) != (float)System.Math.Round((color2.b / 255f), 2) ||
            (float)System.Math.Round((color1.a / 255f), 2) != (float)System.Math.Round((color2.a / 255f), 2))
        {
            return true;
        }
        else
            return false;
    }

    //Class used to house Hue Saturation and Light, since these values are always used together (similar to RGBA in Color)
    [System.Serializable]
    public class HSL : System.IEquatable<HSL>
    {
        public float hue;
        public float saturation;
        public float light;

        public HSL()
        {
            hue = 0;
            saturation = 0;
            light = 0;
        }

        public HSL(float h, float s, float l)
        {
            hue = h;
            saturation = s;
            light = l;
        }
        public override bool Equals(object obj)
        {
            var other = obj as HSL;
            if (other == null) return false;

            return Equals(other);
        }

        // Custom Equals method to compare HSL classes correctly
        public bool Equals(HSL comparedHSL)
        {
            if (comparedHSL == null)
            {
                return false;
            }

            if (ReferenceEquals(this, comparedHSL))
            {
                return true;
            }

            if (hue != comparedHSL.hue || saturation != comparedHSL.saturation || light != comparedHSL.light)
            {
                return false;
            }

            return true;
        }
    }
}
