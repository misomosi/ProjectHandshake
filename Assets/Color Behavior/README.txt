README - COLOR BEHAVIOR:
Brendan Dickinson
Gear5 Media LLC
Created 6/26/2015
Last Updated 8/30/2015
Compatable with Unity 4.5 and up.
--------------------------
First off thank you for purchasing the Color Behavior script and I hope you're excited to use it!  
Please take time to read through this document to understand all the features this asset offers.

Contact / Support:
--------------------------
Mail: Brendan@gear5media.com
Twitter: @Gear5Media

If you find any bugs or if you have any feature requests don't hesitate to shoot me a message!

Website:
--------------------------
http://gear5media.com/

Installation:
--------------------------
Simply bring one of the Color Behavior script(s) (C# or JS) into your project and place it on the object(s) you'd like to have that behavior.

Legal Stuff / Licensing
--------------------------
This asset is licensed under Unity's asset store license for more information you can go here: https://unity3d.com/legal/as_terms
If you finish and release a product in Unity with the Color Behavior asset I would appreciate a mention in the credits. :)

Promotional Videos:
--------------------------
Original - https://www.youtube.com/watch?v=E6pHHL309R4&feature=youtu.be

Description & Features:
--------------------------

Summary:
The Color Behavior asset is a single script that you can attach to objects in your scenes.  I've written both a C# and JavaScript(UnityScript) version
in case people would like to use either version.  This script is meant to make it easier to manipulate the color of objects in your game.  While this
is all in the editor you could change any of the varibles in code during runtime to change the behavior of your object for example when you enter a
trigger.  I've exposed almost everything to the inspector so you can set up each script and object however you'd like without using code at all.

Usage:
Version 1.0 - 6/25/2015
	-Live changing of Color during runtime using the color wheel, rgba values, and even by using a hex value (only available in Unity 5.1.0 or later)
	-Live changing of Color using the inspector sliders for rgba values and the hex value box (this hex value works for all versions of Unity)
	-Live updating Behaviors of color on your object:
		--Grouping - you can put this behavior script on the parent of a group of objects and if group is checked in the inspector the behavior will effect
				the children of the object instead of the object itself.  This behavior will NOT affect the parent object that the script is on.
		--Random - when checked this will give the object a random rgb value without affecting the alpha, if you would like a different random color you can disable and enable this
				checkbox until you find a color you want to use.
			 - this is also useful when used with the Cycle behavior.  If you want the colors to cycle through your list of designated colors randomly instead of linearly.
		--Blink - when checked you cannot use random or cycle.  This will cause your object to blink a random color at the speed of the interval property in seconds.  see interval for
				more details.
		--Cycle - when checked you cannot use blink.  This will cycle through the list of colors linearly (one after another) - cycleList - that YOU must designate in the inspector.  
			- You can do some cool things with this like a blink where it dissappears for an interval by making a color in the list with alpha of 0.  
			- You can use this with Random in order to cycle through the list randomly instead of linearly.
		--Cycle List - list of colors to cycle through when cycle is checked.
		--Speed(Interval) - this measures the amount of time in seconds before the color will change.  So if it is set to 3 then every 3 seconds the color will change.
	-Designation of Shader and/or Material for object or group of object in a single place (useful for rapid testing of groups of objects with different materials and shaders)

Version 2.0 - 8/30/2015
	-All the features of Version 1.0
	-Color easing/tweening from one color to another
	-Grayscale option when not using behaviors (you can also use HSL to accomplish this by setting hue and saturation to 0 and just using the light slider)
	-Hue, Saturation, and Light/Value sliders
	-Shader Property box to designate you're own property (something other than the default _Color property)
	-Shader Properties list that shows all the properties in the current shader that are of type Color
	-Inspector Values are all represented as floats for more precision
	-Better error handling for objects without renderers or the correct shader properties


Extra Notes:
--------------------------
I built this script to help with one of my personal projects but it has evolved since then and will work with almost any type of project.
While this was uploaded with Unity 4.5 and I haven't tested it on older versions it may still work.

-most values are in float for precisions sake (don't let this bother you)
-when light is 0 sat and hue will always be 0 (won't be able to move the sliders)
-when hue is close to 0 it may look like it's a larger number but it's using scientific notation at that exact value
-when clicking on random and then removing it, the color will revert back to whatever color was there before clicking random (by itself not with behaviors)
	-this is the same with several behaviors
-shader properties on a group of objects only looks at the last object shader in the group list