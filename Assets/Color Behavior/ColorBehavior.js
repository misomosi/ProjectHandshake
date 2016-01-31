/* Color Behavior
 * Gear5 Media LLC (Gear5Media.com)
 * Brendan Dickinson
 * 6/25/2015
 * 
 * This script is meant to allow easy customization of colors, tints, and their behaviors 
 * on any object or group of objects.
 * You can specify the material and/or shader that will be used as well as several options
 * for coloring and color behaviour of your object.
 *
 * Updates:
 * 9/12/15
 */

#pragma strict

import System.Collections.Generic;
import System.Collections;
import UnityEngine;

//Color value (using Unity's built in color inspector)
@Header ("Color")
@SerializeField
private var color:Color;
//RGBA slider values RGB going from 0-255, while Alpha goes from 0-1
@Range(0, 255) 
@SerializeField
private var r:float;
@Range(0, 255) 
@SerializeField
private var g:float;
@Range(0, 255) 
@SerializeField
private var b:float;
@Range(0.0f, 1) 
@SerializeField
private var a:float;
//Hex value is the color in hexidecimal
@SerializeField
private var hexValue:String;
//Hue, Saturation, and Light/Value variables
@Range(-1, 5) 
@SerializeField
private var h:float;
@Range(0, 1) 
@SerializeField
private var s:float;
@Range(0, 1) 
@SerializeField
private var l:float;

//Used to give the same behaviour to a group of objects
@Header("Grouping")
@SerializeField
private var group:boolean;
@SerializeField
private var groupList:List.<GameObject> = new List.<GameObject>();

//Interval measures seconds between color changes (used for blink and cycle)
@Header("Speed")
@SerializeField
private var interval:float;

//Booleans to designate active behavior
@Header("Behavior")
@SerializeField
private var random:boolean = false;
@SerializeField
private var easing:boolean = false;
@SerializeField
private var useGrayscale:boolean = false;
@SerializeField
private var blink:boolean = false;
@SerializeField
private var cycle:boolean = false;

//List of colors to cycle through when cycle is the designated behaviour
@SerializeField
private var cycleColors:List.<Color> = new List.<Color>();

//Shader and material properties of the object
@Header("Shader & Material")
@SerializeField
private var shader:Shader;
@SerializeField
private var material:Material;
@SerializeField
private var shaderProperty:String;
@SerializeField
private var shaderProperties:List.<String>;

//Variables used for changing from one behavior to another or updating color
private var previousColor:Color;
private var previousRGBA:Color;
private var currentRGBA:Color;
private var originalColor:Color;
private var targetColor:Color;
private var lerpColor:Color;
private var changedColor:Color;
private var previousHSL:HSL;
private var currentHSL:HSL;
private var previousHex:String;
private var cycleCounter:int;
private var previousCounter:int;	
private var r2:float;
private var g2:float;
private var b2:float;
private var count:float = 0;
private var easeTime:float = 0;
private var needRandom:boolean = false;
private var cycleInitialized:boolean = false;
private var needNextColor:boolean = true;
private var hasRenderer:boolean = true;

//Cache for the renderer of object
private var rendererCache:Renderer;

function Start () {
    //default color options in inspector
    previousColor = Color(0, 0, 0, 0);
    previousRGBA = Color(0, 0, 0, 0);
    previousHSL = HSL();
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
            for (var child:Transform in gameObject.transform)
            {
                groupList.Add(child.gameObject);
            }
    }
    hasRenderer = false;
        //set each object in the group with designated material and shader
        for (var j:int = 0; j < groupList.Count; j++)
        {
            if(groupList[j].GetComponent(Renderer) == null)
            {
                Debug.Log(groupList[j].name + " does not have a renderer and so it was removed from the list.");
                groupList.Remove(groupList[j]);
            }
            else
            {
                rendererCache = groupList[j].GetComponent(Renderer);
                SetMaterialAndShader(rendererCache);
                hasRenderer = true;
            }
        }
    }
    else
    {
        if (gameObject.GetComponent(Renderer) != null)
        {
            rendererCache = gameObject.GetComponent(Renderer);
            SetMaterialAndShader(rendererCache);
        }
        else
        {
            Debug.Log(gameObject.name + " does not have a renderer and so this script cannot be used.");
            hasRenderer = false;
        }
    }

    if(hasRenderer)
    {
        //Using the current renderer find all the properties that are of type color and add them to the Shader Properties list in the inspector
        for (var i:int = 0; i < UnityEditor.ShaderUtil.GetPropertyCount(rendererCache.material.shader); i++)
        {
            if (UnityEditor.ShaderUtil.GetPropertyType(rendererCache.material.shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.Color)
                shaderProperties.Add(UnityEditor.ShaderUtil.GetPropertyName(rendererCache.material.shader, i));
        }

            //Find the initial color and set all other instpector values accordingly
        UpdateColorValues(FindChangedColor());
    }
}

//Use Update for when we are not easing
function Update () {
    if(!easing && hasRenderer)
    {
        if (group)
        {
            for (var i:int = 0; i < groupList.Count; i++)
            {
                rendererCache = groupList[i].GetComponent(Renderer);
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
function FixedUpdate()
{
    if (easing && hasRenderer)
    {
        //when calculating easeTime we divide by this so we don't want it to be 0
        if (interval == 0)
            interval += .00001;

        if (group)
        {
            for (var i = 0; i < groupList.Count; i++)
            {
                rendererCache = groupList[i].GetComponent(Renderer);
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
private function NewRandom()
{
    r2 = Random.Range(0.0f, 1.0f);
    g2 = Random.Range(0.0f, 1.0f);
    b2 = Random.Range(0.0f, 1.0f);
    needRandom = false;
}

//Takes the current rgb values and finds the corresponding HSL values, then returns the HSL object
private function UpdateHSL()
{
    var _R:float = r / 255.0f;
    var _G:float = g / 255.0f;
    var _B:float = b / 255.0f;

    var _Min:float = Mathf.Min(Mathf.Min(_R, _G), _B);
    var _Max:float = Mathf.Max(Mathf.Max(_R, _G), _B);
    var _Delta:float = _Max - _Min;

    var localH:float = 0;
    var localS:float = 0;
    var localL:float = ((_Max + _Min) / 2.0f);

    if (_Delta != 0)
    {
        if (localL < 0.5f)
        {
            localS = (_Delta / (_Max + _Min));
        }
        else
        {
            localS = (_Delta / (2.0f - _Max - _Min));
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

    return HSL(localH, localS, localL);
}

//Takes the given HSL object and returns the corresponding RGBA color
private function HSLToRGB(newHSL:HSL)
{
    var localR:float;
    var localG:float;
    var localB:float;

    if (newHSL.saturation == 0)
    {
        localR = newHSL.light;
        localG = newHSL.light;
        localB = newHSL.light;
    }
    else
    {
        var t1:float;
        var t2:float;
        var th:float = newHSL.hue / 6.0f;

        if (newHSL.light < 0.5d)
        {
            t2 = newHSL.light * (1 + newHSL.saturation);
        }
        else
        {
            t2 = (newHSL.light + newHSL.saturation) - (newHSL.light * newHSL.saturation);
        }
        t1 = 2 * newHSL.light - t2;

        var tr:float;
        var tg:float;
        var tb:float;

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

    return Color(localR, localG, localB, 1);
}

//Used by HSLToRGB to find certain values
private function ColorCalc(c:float, t1:float, t2:float)
{
    if (c < 0) c += 1f;
    if (c > 1) c -= 1f;
    if (6.0 * c < 1.0) return t1 + (t2 - t1) * 6.0f * c;
    if (2.0 * c < 1.0) return t2;
    if (3.0 * c < 2.0) return t1 + (t2 - t1) * (2.0f / 3.0f - c) * 6.0f;
    return t1;
}

//converts a hexidecimal color value to the r, g, b, equivalent
private function HexToColor(hexValue:String)
{
    var localR:float;
    var localG:float;
    var localB:float;
    var subString:String;
    try
    {
        subString = hexValue.Substring(0, 2);
        localR = System.Convert.ToInt32(subString, 16);
        subString = hexValue.Substring(2, 2);
        localG = System.Convert.ToInt32(subString, 16);
        subString = hexValue.Substring(4, 2);
        localB = System.Convert.ToInt32(subString, 16);
        Debug.Log("Finishing Hex Update");
        return Color(localR / 255.0f, localG / 255.0f, localB / 255.0f, 1);
    }
    catch (e)
    {
        Debug.Log("<color=maroon><size=15>THAT'S NOT A HEXIDECIMAL!</size></color>");
        Debug.Log(e);
        return Color(0, 0, 0, 0);
    }
}

//-if no shader or material is designated in the inspector and there's no material on the object
//then it will set the material to default with transparent/diffuse type shader on it
//-if the object has both a material and shader designated it will first set the material, then
//the specified shader (this means the shader on the designated material will be ignored
//-if only the material or shader is designated they will be set to the object respectively
private function SetMaterialAndShader(rendererCache:Renderer)
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
private function SetHexValue(rendererCache:Renderer)
{
    var hexColor:Color;
    previousHex = hexValue;
    hexColor = HexToColor(hexValue);
    rendererCache.material.SetColor(shaderProperty, new Color(hexColor.r / 255.0f, hexColor.g / 255.0f, hexColor.b / 255.0f, hexColor.a));
}

//updates renderer to color in inspector
private function UpdateColor(rendererCache:Renderer, color:Color)
{
    rendererCache.material.SetColor(shaderProperty, color);
    UpdateColor(color);
}

//updates renderer to color in inspector
private function UpdateColor(color:Color)
    {
        r = color.r * 255.0f;
        g = color.g * 255.0f;
        b = color.b * 255.0f;
        a = color.a;
    }

//live updates hex value as rgba value changes
private function UpdateHex()
{
    var rToInt:int = r;
    var gToInt:int = g;
    var bToInt:int = b;

    var hexString:String;
    hexString = rToInt.ToString("X2") + gToInt.ToString("X2") + bToInt.ToString("X2");
    return hexString;
}

//cycle to the next color in the cycleList
private function CycleColor(rendererCache:Renderer)
{
    if (cycleCounter == cycleColors.Count)
        cycleCounter = 0;
    //if random is also selected cycle through cycleColors list randomly
    //else cycle linearly from beginning to end of the list.
    if (random)
    {
        previousCounter = cycleCounter;
        //finds a new color in the list
        cycleCounter = Random.Range(0, cycleColors.Count);
        while (cycleCounter == previousCounter && cycleColors.Count > 1) 
        {
        	cycleCounter = Random.Range(0, cycleColors.Count);
        }
        rendererCache.material.color = cycleColors[cycleCounter];
    }
    else
    {
        rendererCache.material.color = cycleColors[cycleCounter];
        cycleCounter++;
    }
    //if cycle is true blink should not be true & vice versa
    blink = false;
    count = 0;
}

//continually ease to the next color in the cycleList
private function EasingCycleColor(rendererCache:Renderer)
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
private function EasingBlinkColor(rendererCache:Renderer)
{
    InitializeBlinkEasing(rendererCache);
    LerpColor();
    //if random is also selected cycle through cycleColors list randomly
    //else cycle linearly from beginning to end of the list.
    if (lerpColor == targetColor)
        SetNextLerpColor();
}

private function InitializeCycleEasing(rendererCache:Renderer)
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

private function InitializeBlinkEasing(rendererCache:Renderer)
{
    //set up initial values
    if (needNextColor)
    {
        originalColor = rendererCache.material.GetColor(shaderProperty);
        targetColor = Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), a);

        //if blink is true cycle and random should be false
        random = false;
        cycle = false;
        needRandom = false;
        needNextColor = false;
    }
}

private function LerpColor()
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

private function SetNextLerpColor()
{
    originalColor = targetColor;
    if (blink)
    {
        targetColor = Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), a);
    }
    else if (cycle)
    {
        //if random is also selected pick next color in cycleList randomly
        //else cycle linearly from beginning to end of the list.
        if (random)
        {
            previousCounter = cycleCounter;
            //finds a new color in the list
            cycleCounter = Random.Range(0, cycleColors.Count);
            while (cycleCounter == previousCounter && cycleColors.Count > 1) 
            {
                cycleCounter = Random.Range(0, cycleColors.Count);
            }
        }
        else
            cycleCounter++;
    }
    easeTime = 0;
}

//blink to random color at interval speed
private function BlinkColor(rendererCache:Renderer)
{
    //if blink is true cycle and random should be false
    random = false;
    cycle = false;
    rendererCache.material.SetColor(shaderProperty, Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), a));
    count = 0;
    needRandom = false;
}

//set color to a random rgb value that is generated
private function SetRandomColor(rendererCache:Renderer)
{
    if (needRandom == true)
    {
        NewRandom();
    }
    blink = false;
    rendererCache.material.SetColor(shaderProperty, Color(r2, g2, b2, a));
    UpdateColorValues(rendererCache.material.GetColor(shaderProperty));
}

//Determine what behavior to do and call that function
private function DoColorBehavior(rendererCache:Renderer)
{
    if (!random)
    {
        needRandom = true;
    }
    //if cycle is on, cycle through the cycleColors list if it's not empty
    if (cycle && count >= interval && cycleColors.Count > 0 || easing && cycle && cycleColors.Count > 0)
    {
        if(!easing)
            CycleColor(rendererCache);
        else
            EasingCycleColor(rendererCache);
    }
    //if blink is true blink through random colors at the designated interval speed (seconds)
    else if (blink && count >= interval || easing && blink)
    {
        if(!easing)
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
private function UpdateColorValues(newColor:Color)
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
private function UpdateRenderer()
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
        Debug.Log("That Shader Property Doesn't Exist!");
}

//finds the most recently changed color value
private function FindChangedColor()
{
    currentRGBA = Color(r / 255f, g / 255f, b / 255f, a);
    currentHSL = HSL(h, s, l);

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
        changedColor = Color(changedColor.grayscale, changedColor.grayscale, changedColor.grayscale);
    }
    return changedColor;
}

    //compares two colors and returns true if they are at all different
public function AreNotSameColor(color1:Color, color2:Color)
{
    //rounds numbers so they don't have to be exactly the same but just close enough
    if (Mathf.Round((color1.r / 255f)*100)/100 != Mathf.Round((color2.r / 255f) *100)/100 || Mathf.Round((color1.g / 255f)*100)/100 != Mathf.Round((color2.g / 255f)*100)/100 || Mathf.Round((color1.b / 255f)*100)/100 != Mathf.Round((color2.b / 255f)*100)/100 || Mathf.Round((color1.a / 255f)*100)/100 != Mathf.Round((color2.a / 255f)*100)/100)
    {
        return true;
    }
    else
        return false;
}

//Class used to house Hue Saturation and Light, since these values are always used together (similar to RGBA in Color)
@System.Serializable
private class HSL{
    public var hue:float;
    public var saturation:float;
    public var light:float;

    function HSL()
    {
        hue = 0;
        saturation = 0;
        light = 0;
    }

    function HSL(h:float, s:float, l:float)
    {
        hue = h;
        saturation = s;
        light = l;
    }

    // Custom Equals method to compare HSL classes correctly
    function Equals(comparedHSL:HSL)
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





