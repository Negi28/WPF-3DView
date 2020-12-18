﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PRC_Phatv_3DView.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PRC_Phatv_3DView.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///out vec4 FragColor;
        ///
        ///void main()
        ///{
        ///    FragColor = vec4(1.0); // set all 4 vector values to 1.0
        ///}.
        /// </summary>
        internal static string fragLightShader {
            get {
                return ResourceManager.GetString("fragLightShader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///out vec4 FragColor;
        ///
        ///uniform vec3 objectColor;
        ///uniform vec3 lightColor;
        ///uniform vec3 lightPos;
        ///uniform vec3 viewPos;
        ///
        ///in vec3 Normal; //The normal of the fragment is calculated in the vertex shader.
        ///in vec3 FragPos; //The fragment position.
        ///
        ///void main()
        ///{
        ///    float ambientStrength = 0.0;
        ///    vec3 ambient = ambientStrength * lightColor;
        ///
        ///     //We calculate the light direction, and make sure the normal is normalized.
        ///    vec3 norm = normalize(Normal);
        ///    vec3 lightDir = n [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string fragmentShader {
            get {
                return ResourceManager.GetString("fragmentShader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///out vec4 FragColor;
        ///in vec3 vs_color;
        ///
        ///void main()
        ///{
        ///    FragColor = vec4(vs_color, 1.0);
        ///}.
        /// </summary>
        internal static string fragmentShader_color {
            get {
                return ResourceManager.GetString("fragmentShader_color", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///out vec4 FragColor;
        ///
        ///uniform vec3 objectColor;
        ///uniform vec3 lightColor;
        ///uniform vec3 lightdirection;
        ///uniform vec3 viewPos;
        ///
        ///in vec3 Normal; //The normal of the fragment is calculated in the vertex shader.
        ///in vec3 FragPos; //The fragment position.
        ///
        ///void main()
        ///{
        ///    float ambientStrength = 0.0;
        ///    vec3 ambient = ambientStrength * lightColor;
        ///
        ///     //We calculate the light direction, and make sure the normal is normalized.
        ///    vec3 norm = normalize(Normal);
        ///    vec3 lightD [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string fragmentShader_directional_light {
            get {
                return ResourceManager.GetString("fragmentShader_directional_light", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        /////The material is a collection of some values that we talked about in the last tutorial,
        /////some crucial elements to the phong model.
        ///struct Material {
        ///    vec3 ambient;
        ///    vec3 diffuse;
        ///    vec3 specular;
        ///
        ///    float shininess; //Shininess is the power the specular light is raised to
        ///};
        /////The light contains all the values from the light source, how the ambient diffuse and specular values are from the light source.
        /////This is technically what we were using in the last episode as w [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string fragmentShader_material_light {
            get {
                return ResourceManager.GetString("fragmentShader_material_light", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///out vec4 FragColor;
        ///
        ///struct BasicLight {
        ///    vec3 lightPos;
        ///    float ambientStrength;
        ///    float deffuseStrength;
        ///    float specularStrength;
        ///    vec3 lightColor;
        ///};
        ///
        ///#define NUM_BASIC_LIGHT 5
        ///
        ///uniform vec3 objectColor;
        ///uniform vec3 viewPos;
        ///uniform BasicLight bsLight[NUM_BASIC_LIGHT];
        ///
        ///in vec3 Normal;
        ///in vec3 FragPos;
        ///
        ///vec3 CalcDirLight(BasicLight light, vec3 normal, vec3 viewDir);
        ///
        ///void main()
        ///{
        ///    vec3 norm = normalize(Normal);
        ///    vec3 viewDir = normalize(view [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string fragmentShader_multiple_basic_light {
            get {
                return ResourceManager.GetString("fragmentShader_multiple_basic_light", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout(location = 0) in vec3 aPosition;
        ///layout(location = 1) in vec3 aNormal;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///out vec3 Normal;
        ///out vec3 FragPos;
        ///
        ///void main(void)
        ///{    
        ///    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        ///    FragPos = vec3(vec4(aPosition, 1.0) * model);
        ///    Normal = aNormal * mat3(transpose(inverse(model)));
        ///}
        ///.
        /// </summary>
        internal static string vertexShader {
            get {
                return ResourceManager.GetString("vertexShader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout(location = 0) in vec3 aPosition;
        ///layout(location = 1) in vec3 color;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///out vec3 vs_color;
        ///
        ///void main(void)
        ///{    
        ///    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        ///	vs_color = color;
        ///}
        ///.
        /// </summary>
        internal static string vertexShader_color {
            get {
                return ResourceManager.GetString("vertexShader_color", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout(location = 0) in vec3 aPosition;
        ///layout(location = 1) in vec3 aNormal;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///out vec3 Normal;
        ///out vec3 FragPos;
        ///
        ///void main(void)
        ///{    
        ///    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        ///    FragPos = vec3(vec4(aPosition, 1.0) * model);
        ///    Normal = aNormal * mat3(transpose(inverse(model)));
        ///}
        ///.
        /// </summary>
        internal static string vertexShader_directional_light {
            get {
                return ResourceManager.GetString("vertexShader_directional_light", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout(location = 0) in vec3 aPosition;
        ///layout(location = 1) in vec3 aNormal;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///out vec3 Normal;
        ///out vec3 FragPos;
        ///
        ///void main(void)
        ///{    
        ///    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        ///    FragPos = vec3(vec4(aPosition, 1.0) * model);
        ///    Normal = aNormal * mat3(transpose(inverse(model)));
        ///}
        ///.
        /// </summary>
        internal static string vertexShader_material_light {
            get {
                return ResourceManager.GetString("vertexShader_material_light", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout(location = 0) in vec3 aPosition;
        ///layout(location = 1) in vec3 aNormal;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///out vec3 Normal;
        ///out vec3 FragPos;
        ///
        ///void main(void)
        ///{    
        ///    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
        ///    FragPos = vec3(vec4(aPosition, 1.0) * model);
        ///    Normal = aNormal * mat3(transpose(inverse(model)));
        ///}
        ///.
        /// </summary>
        internal static string vertexShader_multiple_basic_light {
            get {
                return ResourceManager.GetString("vertexShader_multiple_basic_light", resourceCulture);
            }
        }
    }
}