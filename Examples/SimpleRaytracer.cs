// Based on http://www.coldcity.com/index.php/simple-csharp-raytracer/
// Original license comment follows

/*
 * simpleray
 * A simple raytracer for teaching purposes 
 * 
 * IainC, 2009
 * License: Do WTF you want
 * 
 * World coord system:
 *  Origin (0,0,0) is the center of the screen
 *  X increases towards right of screen
 *  Y increases towards top of screen
 *  Z increases into screen
 *  
 * Enough vector maths to get you through:
 *  - The dot product of two vectors gives the cosine of the angle between them
 *  - Normalisation is scaling a vector to have magnitude 1: makes it a "unit vector"
 *  - To get a unit direction vector from point A to point B, do B-A and normalise the result
 *  - To move n units along a direction vector from an origin, new position = origin + (direction * n)
 *  - To reflect a vector in a surface with a known surface normal:
 *          negativeVec = -vecToReflect;
 *          reflectedVec = normal * (2.0f * negativeVec.Dot(normal)) - negativeVec;
 */

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace simpleray {
    public class Vector3f {
        public float x, y, z;

        public Vector3f(float x = 0, float y = 0, float z = 0) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float Dot(Vector3f b) {
            return (x * b.x + y * b.y + z * b.z);
        }

        public void Normalise() {
            float f = (float)(1.0f / Math.Sqrt(this.Dot(this)));

            x *= f;
            y *= f;
            z *= f;
        }

        public float Magnitude() {
            return (float)Math.Sqrt(x*x + y*y + z*z);
        }

        public static Vector3f operator -(Vector3f a, Vector3f b) {
            return new Vector3f(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3f operator -(Vector3f a) {
            return new Vector3f(-a.x, -a.y, -a.z);
        }

        public static Vector3f operator *(Vector3f a, float b) {
            return new Vector3f(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3f operator /(Vector3f a, float b) {
            return new Vector3f(a.x / b, a.y / b, a.z / b);
        }

        public static Vector3f operator +(Vector3f a, Vector3f b) {
            return new Vector3f(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public Vector3f ReflectIn(Vector3f normal) {
            Vector3f negVector = -this;
            Vector3f reflectedDir = normal * (2.0f * negVector.Dot(normal)) - negVector;
            return reflectedDir;
        }
    }
    
    public class Light {
        public Vector3f position;

        public Light(Vector3f p) {
            position = p;
        }
    }
    
    public class Ray {
        public const float WORLD_MAX = 1000.0f;

        public Vector3f origin;
        public Vector3f direction;

        public RTObject closestHitObject;
        public float closestHitDistance;
        public Vector3f hitPoint;

        public Ray(Vector3f o, Vector3f d) {
            origin = o;
            direction = d;
            closestHitDistance = WORLD_MAX;
            closestHitObject = null;
        }
    }
    
    public abstract class RTObject {
        public Color color;

        public abstract float Intersect(Ray ray);

        public abstract Vector3f GetSurfaceNormalAtPoint(Vector3f p);
    }
    
    class Sphere : RTObject {
        // to specify a sphere we need it's position and radius
        public Vector3f position;
        public float radius;

        public Sphere(Vector3f p, float r, Color c) {
            position = p;
            radius = r;
            color = c;
        }

        public override float Intersect(Ray ray) {
            Vector3f lightFromOrigin = position - ray.origin;               // dir from origin to us
            float v = lightFromOrigin.Dot(ray.direction);                   // cos of angle between dirs from origin to us and from origin to where the ray's pointing

            float hitDistance = 
                radius * radius + v * v - 
                lightFromOrigin.x * lightFromOrigin.x - 
                lightFromOrigin.y * lightFromOrigin.y - 
                lightFromOrigin.z * lightFromOrigin.z;

            if (hitDistance < 0)                                            // no hit (do this check now before bothering to do the sqrt below)
                return -1;

            hitDistance = v - (float)Math.Sqrt(hitDistance);			    // get actual hit distance

            if (hitDistance < 0)
                return -1;
            else
                return (float)hitDistance;
        }

        public override Vector3f GetSurfaceNormalAtPoint(Vector3f p) {
            Vector3f normal = p - position;
            normal.Normalise();
            return normal;
        }
    }
    
    class Plane : RTObject {
        public Vector3f normal;
        public float distance;

        public Plane(Vector3f n, float d, Color c) {
            normal = n;
            distance = d;
            color = c;
        }

        public override float Intersect(Ray ray) {
            float normalDotRayDir = normal.Dot(ray.direction);
            if (normalDotRayDir == 0)   // Ray is parallel to plane (this early-out won't help very often!)
                return -1;

            // Any none-parallel ray will hit the plane at some point - the question now is just
            // if it in the positive or negative ray direction.
            float hitDistance = -(normal.Dot(ray.origin) - distance) / normalDotRayDir;

            if (hitDistance < 0)        // Ray dir is negative, ie we're behind the ray's origin
                return -1;
            else 
                return hitDistance;
        }

        public override Vector3f GetSurfaceNormalAtPoint(Vector3f p) {
            return normal;              // This is of course the same across the entire plane
        }
    }
    
    class RayTracer {
        const float PI =        3.1415926536f;                                  // maths constants
        const float PI_X_2 =    6.2831853072f;
        const float PI_OVER_2 = 1.5707963268f;

        const int CANVAS_WIDTH = 400;                                          // output image dimensions
        const int CANVAS_HEIGHT = 300;
        
        const float TINY = 0.0001f;                                             // a very short distance in world space coords
        const int MAX_DEPTH = 3;                                                // max recursion for reflections
        
        const float MATERIAL_DIFFUSE_COEFFICIENT = 0.5f;                        // material diffuse brightness
        const float MATERIAL_REFLECTION_COEFFICIENT = 0.5f;                     // material reflection brightness
        const float MATERIAL_SPECULAR_COEFFICIENT = 2.0f;                       // material specular highlight brightness
        const float MATERIAL_SPECULAR_POWER = 50.0f;                            // material shininess (higher values=smaller highlights)
        static Color BG_COLOR = Color.BlueViolet;                               // scene bg colour
        
        static Vector3f eyePos = new Vector3f(0, 0, -5.0f);                     // eye pos in world space coords
        static Vector3f screenTopLeftPos = new Vector3f(-6.0f, 4.0f, 0);        // top-left corner of screen in world coords
        static Vector3f screenBottomRightPos = new Vector3f(6.0f, -4.0f, 0);    // bottom-right corner of screen in world coords
        
        static float pixelWidth, pixelHeight;                                   // dimensions of screen pixel **in world coords**

        static List<RTObject> objects;                                          // all RTObjects in the scene
        static List<Light> lights;                                              // all lights
        static Random random;                                                   // global random for repeatability

        static void Main(string[] args) {
            // init structures
            objects = new List<RTObject>();
            lights = new List<Light>();
            random = new Random(01478650229);
            Bitmap canvas = new Bitmap(CANVAS_WIDTH, CANVAS_HEIGHT);
           
            // add some objects
            for (int i = 0; i < 30; i++) {
                float x = (float)(random.NextDouble() * 10.0f) - 5.0f;          // Range -5 to 5
                float y = (float)(random.NextDouble() * 10.0f) - 5.0f;          // Range -5 to 5
                float z = (float)(random.NextDouble() * 10.0f);                 // Range 0 to 10
                Color c = Color.FromArgb(255, random.Next(255), random.Next(255), random.Next(255));
                Sphere s = new Sphere(new Vector3f(x, y, z), (float)(random.NextDouble()), c);
                objects.Add(s);
            }
            //Sphere debugSphere = new Sphere(new Vector3f(0, 0, 5.0f), 0.2f, Color.ForestGreen);
            //objects.Add(debugSphere);
            Plane floor = new Plane(new Vector3f(0, 1.0f, 0), -10.0f, Color.Aquamarine);
            objects.Add(floor);
            
            // add some lights
            lights.Add(new Light(new Vector3f(2.0f, 0.0f, 0)));
            lights.Add(new Light(new Vector3f(0, 10.0f, 7.5f)));

            // calculate width and height of a pixel in world space coords
            pixelWidth = (screenBottomRightPos.x - screenTopLeftPos.x) / CANVAS_WIDTH;
            pixelHeight = (screenTopLeftPos.y - screenBottomRightPos.y) / CANVAS_HEIGHT;

            // render it
            int dotPeriod = CANVAS_HEIGHT / 10;
            System.Console.WriteLine("Rendering...\n");
            System.Console.WriteLine("|0%---100%|");

            RenderRow(canvas, dotPeriod, 0);

            // save the pretties
            canvas.Save("output.png");
        }
        
        static void RenderRow (System.Drawing.Bitmap canvas, int dotPeriod, int y) {
            if ((y % dotPeriod) == 0) System.Console.Write("*");
          
            for (int x = 0; x < CANVAS_WIDTH; x++) {
                Color c = RenderPixel(x, y);
                canvas.SetPixel(x, y, c);
            }
            
            if (y >= CANVAS_HEIGHT)
              return;
            
            SetTimeout(1, () => 
              RenderRow(canvas, dotPeriod, y + 1)
            );
        }
        
        static void SetTimeout (int timeoutMs, Action action) {
          JSIL.Verbatim.Eval(@"
            setTimeout(action, timeoutMs);
            return
          ");
          
          action();
        }

        // Given a ray with origin and direction set, fill in the intersection info
        static void CheckIntersection(ref Ray ray) {
            foreach (RTObject obj in objects) {                     // loop through objects, test for intersection
                float hitDistance = obj.Intersect(ray);             // check for intersection with this object and find distance
                if (hitDistance < ray.closestHitDistance && hitDistance > 0) {
                    ray.closestHitObject = obj;                     // object hit and closest yet found - store it
                    ray.closestHitDistance = hitDistance;
                }
            }

            ray.hitPoint = ray.origin + (ray.direction * ray.closestHitDistance);   // also store the point of intersection 
        }

        // raytrace a pixel (ie, set pixel color to result of a trace of a ray starting from eye position and
        // passing through the world coords of the pixel)
        static Color RenderPixel(int x, int y) {
            // First, calculate direction of the current pixel from eye position
            float sx = screenTopLeftPos.x + (x * pixelWidth);
            float sy = screenTopLeftPos.y - (y * pixelHeight);
            Vector3f eyeToPixelDir = new Vector3f(sx, sy, 0) - eyePos;
            eyeToPixelDir.Normalise();

            // Set up primary (eye) ray
            Ray ray = new Ray(eyePos, eyeToPixelDir);

            // And trace it!
            return Trace(ray, 0);
        }

        // given a ray, trace it into the scene and return the colour of the surface it hits 
        // (handles reflections recursively)
        static Color Trace(Ray ray, int traceDepth) {
            // See if the ray intersected an object
            CheckIntersection(ref ray);
            if (ray.closestHitDistance >= Ray.WORLD_MAX || ray.closestHitObject == null) // No intersection
                return BG_COLOR;
            
            // Got a hit - set initial colour to ambient light
            float r = 0.15f * ray.closestHitObject.color.R;
            float g = 0.15f * ray.closestHitObject.color.G; 
            float b = 0.15f * ray.closestHitObject.color.B;

            // Set up stuff we'll need for shading calcs
            Vector3f surfaceNormal = ray.closestHitObject.GetSurfaceNormalAtPoint(ray.hitPoint);
            Vector3f viewerDir = -ray.direction;                            // Direction back to the viewer (simply negative of ray dir)

            // Loop through the lights, adding contribution of each
            foreach (Light light in lights) {
                Vector3f lightDir = new Vector3f();
                float lightDistance;

                // Find light direction and distance
                lightDir = light.position - ray.hitPoint;               // Get direction to light
                lightDistance = lightDir.Magnitude();
                //lightDir = lightDir / lightDistance;                  // Light exponential falloff
                lightDir.Normalise();
                
                // Shadow check: check if this light's visible from the point
                // NB: Step out slightly from the hitpoint first
                Ray shadowRay = new Ray(ray.hitPoint + (lightDir * TINY), lightDir);
                shadowRay.closestHitDistance = lightDistance;           // IMPORTANT: We only want it to trace as far as the light!
                CheckIntersection(ref shadowRay);
                if (shadowRay.closestHitObject != null)                 // We hit something -- ignore this light entirely
                    continue;

                float cosLightAngleWithNormal = surfaceNormal.Dot(lightDir);

                if (MATERIAL_DIFFUSE_COEFFICIENT > TINY) {
                    // Calculate light's diffuse component - note that this is view independant
                    // Dot product of surface normal and light direction gives cos of angle between them so will be in 
                    // range -1 to 1. We use that as a scaling factor; common technique, called "cosine shading".
                    if (cosLightAngleWithNormal <= 0) continue;

                    // Add this light's diffuse contribution to our running totals
                    r += MATERIAL_DIFFUSE_COEFFICIENT * cosLightAngleWithNormal * ray.closestHitObject.color.R;
                    g += MATERIAL_DIFFUSE_COEFFICIENT * cosLightAngleWithNormal * ray.closestHitObject.color.G;
                    b += MATERIAL_DIFFUSE_COEFFICIENT * cosLightAngleWithNormal * ray.closestHitObject.color.B;
                }

                if (MATERIAL_SPECULAR_COEFFICIENT > TINY) {
                    // Specular component - dot product of light's reflection vector and viewer direction
                    // Direction to the viewer is simply negative of the ray direction
                    Vector3f lightReflectionDir = surfaceNormal * (cosLightAngleWithNormal * 2) - lightDir;
                    float specularFactor = viewerDir.Dot(lightReflectionDir);
                    if (specularFactor > 0) {
                        // To get smaller, sharper highlights we raise it to a power and multiply it
                        specularFactor = MATERIAL_SPECULAR_COEFFICIENT * (float)Math.Pow(specularFactor, MATERIAL_SPECULAR_POWER);

                        // Add the specular contribution to our running totals
                        r += specularFactor * ray.closestHitObject.color.R;
                        g += specularFactor * ray.closestHitObject.color.G;
                        b += specularFactor * ray.closestHitObject.color.B;
                    }
                }
            }
        
            // Now do reflection, unless we're too deep
            if (traceDepth < MAX_DEPTH && MATERIAL_REFLECTION_COEFFICIENT > TINY) {
                // Set up the reflected ray - notice we move the origin out a tiny bit again
                Vector3f reflectedDir = ray.direction.ReflectIn(surfaceNormal);
                Ray reflectionRay = new Ray(ray.hitPoint + reflectedDir * TINY, reflectedDir);

                // And trace!
                Color reflectionCol = Trace(reflectionRay, traceDepth + 1);

                // Add reflection results to running totals, scaling by reflect coeff.
                r += MATERIAL_REFLECTION_COEFFICIENT * reflectionCol.R;
                g += MATERIAL_REFLECTION_COEFFICIENT * reflectionCol.G;
                b += MATERIAL_REFLECTION_COEFFICIENT * reflectionCol.B;
            }

            // Clamp RGBs
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            return (Color.FromArgb(255, (int)r, (int)g, (int)b));
        }
    }
}
