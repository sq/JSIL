// Based on http://www.coldcity.com/index.php/simple-csharp-pathtracer/
// Original license comment follows

/*
 * simplepath
 * A simple pathtracer for teaching purposes 
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
        
        // dot product -- returns the cosine of the angle between two vectors
        public float Dot(Vector3f b) {
            return (x * b.x + y * b.y + z * b.z);
        }

        // normalise -- scale magnitude of vector to 1. used a lot to construct a point on
        // a unit sphere which represents a direction
        public void Normalise() {
            float f = (float)(1.0f / Math.Sqrt(this.Dot(this)));

            x *= f;
            y *= f;
            z *= f;
        }

        // return the length of the vector
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

        public static Vector3f CrossProduct(Vector3f v1, Vector3f v2)
        {
            Vector3f v = new Vector3f();

            v.x = v1.y * v2.z - v1.z * v2.y;
            v.y = v1.z * v2.x - v1.x * v2.z;
            v.z = v1.x * v2.y - v1.y * v2.x;

            return v;
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
        public Color color;     // Surface colour
        public bool isEmitter;  // If true, this object's an emitter

        // return distance at which this object is intersected by a ray, or -1 if no intersection
        public abstract float Intersect(Ray ray);

        // return the surface normal (perpendicular vector to the surface) for a given point on the surface on the object
        public abstract Vector3f GetSurfaceNormalAtPoint(Vector3f p);
    }
    
    class Plane : RTObject {
        // a plane can be specified with just it's surface normal and an offset from the origin in the 
        // direction of the normal
        public Vector3f normal;
        public float distance;

        public Plane(Vector3f n, float d, Color c) {
            normal = n;
            distance = d;
            color = c;
            isEmitter = false;
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

    class Sphere : RTObject {
        // to specify a sphere we need it's position and radius
        public Vector3f position;
        public float radius;

        public Sphere(Vector3f p, float r, Color c) {
            position = p;
            radius = r;
            color = c;
            isEmitter = false;
        }

        public override float Intersect(Ray ray) {
            Vector3f lightFromOrigin = position - ray.origin;               // dir from origin to us
            float v = lightFromOrigin.Dot(ray.direction);                   // cos of angle between dirs from origin to us and from origin to where the ray's pointing

            float hitDistance = radius * radius + v * v - lightFromOrigin.x * lightFromOrigin.x - lightFromOrigin.y * lightFromOrigin.y - lightFromOrigin.z * lightFromOrigin.z;

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
    
    class Renderer {
        const int CANVAS_WIDTH = 320;                                           // output image dimensions
        const int CANVAS_HEIGHT = 240;
        
        const float TINY = 0.0001f;                                             // a very short distance in world space coords
        const int MAX_DEPTH = 4;                                                // max recursion for reflections
        const int RAYS_PER_PIXEL = 512;                                         // how many rays to shoot per pixel?

        static Vector3f eyePos = new Vector3f(0, 2.0f, -5.0f);                  // eye pos in world space coords
        static Vector3f screenTopLeftPos = new Vector3f(-4.0f, 5.5f, 0);        // top-left corner of screen in world coords - note aspect ratio should match image
        static Vector3f screenBottomRightPos = new Vector3f(4.0f, -0.5f, 0);    // bottom-right corner of screen in world coords
        
        static float pixelWidth, pixelHeight;                                   // dimensions of screen pixel **in world coords**

        static List<RTObject> objects;                                          // all RTObjects in the scene
        static Random random;                                                   // global random for repeatability

        static void Main(string[] args) {
            // init structures
            objects = new List<RTObject>();
            random = new Random(45734);
            Bitmap canvas = new Bitmap(CANVAS_WIDTH, CANVAS_HEIGHT);
            
            // add some objects
            Sphere s = new Sphere(new Vector3f(-2.0f, 2.0f, 0), 1.0f, Color.FromArgb(255, 127, 0, 0));
            objects.Add(s);
            s = new Sphere(new Vector3f(0, 2.0f, 0), 1.0f, Color.OldLace);
            s.isEmitter = true; // this one's a light source
            objects.Add(s);
            s = new Sphere(new Vector3f(2.0f, 2.0f, 0), 1.0f, Color.FromArgb(255, 0, 127, 0));
            objects.Add(s);

            // ceiling and floor
            // pathtracing needs things for photons to bounce off! otherwise
            // most photons exit the scene early before doing their max
            // number of bounces
            Plane floor = new Plane(new Vector3f(0, 1.0f, 0), 1.0f, Color.FromArgb(255, 200, 200, 200));
            objects.Add(floor);
            Plane ceiling = new Plane(new Vector3f(0, -1.0f, 0), -5.0f, Color.FromArgb(255, 200, 200, 200));
            objects.Add(ceiling);
            Plane leftWall = new Plane(new Vector3f(1.0f, 0, 0), -3.0f, Color.FromArgb(255, 75, 75, 200));
            objects.Add(leftWall);
            Plane rightWall = new Plane(new Vector3f(-1.0f, 0, 0), -3.0f, Color.FromArgb(255, 200, 75, 75));
            objects.Add(rightWall);
            Plane backWall = new Plane(new Vector3f(0, 0, -1), -3.0f, Color.FromArgb(255, 200, 200, 200));
            objects.Add(backWall);

            // calculate width and height of a pixel in world space coords
            pixelWidth = (screenBottomRightPos.x - screenTopLeftPos.x) / CANVAS_WIDTH;
            pixelHeight = (screenTopLeftPos.y - screenBottomRightPos.y) / CANVAS_HEIGHT;

            // render it
            int dotPeriod = CANVAS_HEIGHT / 20;
            System.Console.WriteLine("Rendering...\n");
            System.Console.WriteLine("|0%-----------100%|"); 
            
            RenderRow(canvas, dotPeriod, 0);

            // save the pretties
            canvas.Save("output.png");
        }
        
        static void RenderRow (System.Drawing.Bitmap canvas, int dotPeriod, int y) {            
            if (y >= CANVAS_HEIGHT)
                return;
            
            if ((y % dotPeriod) == 0) 
                System.Console.Write("*");
          
            for (int x = 0; x < CANVAS_WIDTH; x++) {
                Color c = RenderPixel(x, y);
                canvas.SetPixel(x, y, c);
            }
            
            SetTimeout(0, () => 
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

        // render a pixel (ie, set pixel color to result of a trace of a ray starting from eye position and
        // passing through the world coords of the pixel)
        static Color RenderPixel(int x, int y) {
            // First, calculate direction of the current pixel from eye position
            float sx = screenTopLeftPos.x + (x * pixelWidth);
            float sy = screenTopLeftPos.y - (y * pixelHeight);
            Vector3f eyeToPixelDir = new Vector3f(sx, sy, 0) - eyePos;
            eyeToPixelDir.Normalise();

            // Set up primary (eye) ray
            Ray ray = new Ray(eyePos, eyeToPixelDir);

            // And send a bunch of reverse photons that way!
            // Since each photon we send into Trace with a depth of 0 will
            // bounce around randomly, we need to send many photons into 
            // every pixel to get good convergence
            float r = 0, g = 0, b = 0;
            for (int i = 0; i < RAYS_PER_PIXEL; i++) {
                Color c = Trace(ray, 1);
                r += c.R;
                g += c.G;
                b += c.B;
            }
            r /= RAYS_PER_PIXEL;
            g /= RAYS_PER_PIXEL;
            b /= RAYS_PER_PIXEL;
            return (Color.FromArgb(255, (int)r, (int)g, (int)b));
        }

        // given a ray, trace it into the scene and return the colour of the surface it hits 
        // (handles bounces recursively)
        static Color Trace(Ray ray, int traceDepth) {
            // See if the ray intersected an object (only if it hasn't already got one - we don't need to
            // recalculate the first intersection for each sample on the same pixel!)
            if (ray.closestHitObject == null)
                CheckIntersection(ref ray);
        
            if (ray.closestHitDistance >= Ray.WORLD_MAX || ray.closestHitObject == null) // No intersection
                return Color.Black;
            
            // Got a hit - was it an emitter? If so just return the emitter's colour
            if (ray.closestHitObject.isEmitter)
                return ray.closestHitObject.color;

            if (traceDepth >= MAX_DEPTH) 
                return Color.Black;

            // Get surface normal at intersection
            Vector3f surfaceNormal = ray.closestHitObject.GetSurfaceNormalAtPoint(ray.hitPoint);
            
            // Pick a point on a hemisphere placed on the intersection point (of which 
            // the surface normal is the north pole)
            if (surfaceNormal.Dot(ray.direction) >= 0)
                surfaceNormal = surfaceNormal * -1.0f;
            float r1 = (float)(random.NextDouble() * Math.PI * 2.0f);
            float r2 = (float)random.NextDouble();
            float r2s = (float)Math.Sqrt(r2);
            Vector3f u = new Vector3f(1.0f, 0, 0);
            if (Math.Abs(surfaceNormal.x) > 0.1f) {
                u.x = 0;
                u.y = 1.0f;
            }
            u = Vector3f.CrossProduct(u, surfaceNormal);
            u.Normalise();
            Vector3f v = Vector3f.CrossProduct(u, surfaceNormal);
            
            // Now set up a direction from the hitpoint to that chosen point
            Vector3f reflectionDirection = (u * (float)Math.Cos(r1) * r2s  +  v * (float)Math.Sin(r1) * r2s  +  surfaceNormal * (float)Math.Sqrt(1 - r2));
            reflectionDirection.Normalise();

            // And follow that path (note that we're not spawning a new ray -- just following the one we were
            // originally passed for MAX_DEPTH jumps)
            Ray reflectionRay = new Ray(ray.hitPoint, reflectionDirection);
            Color reflectionCol = Trace(reflectionRay, traceDepth + 1);
            
            // Now factor the colour we got from the reflection
            // into this object's own colour; ie, illuminate
            // the current object with the results of that reflection
            float r = ray.closestHitObject.color.R * reflectionCol.R;
            float g = ray.closestHitObject.color.G * reflectionCol.G;
            float b = ray.closestHitObject.color.B * reflectionCol.B;

            r /= 255.0f;
            g /= 255.0f;
            b /= 255.0f;

            return (Color.FromArgb(255, (int)r, (int)g, (int)b));
        }
    }
}