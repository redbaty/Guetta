﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Guetta.Localisation.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Language {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Language() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Guetta.Localisation.Resources.Language", typeof(Language).Assembly);
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
        ///   Looks up a localized string similar to Can&apos;t skip songs.
        /// </summary>
        internal static string CantSkip {
            get {
                return ResourceManager.GetString("CantSkip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid arguments.
        /// </summary>
        internal static string InvalidArgument {
            get {
                return ResourceManager.GetString("InvalidArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No songs in queue.
        /// </summary>
        internal static string NoSongsInQueue {
            get {
                return ResourceManager.GetString("NoSongsInQueue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} you&apos;re not in a voice channel.
        /// </summary>
        internal static string NotInChannel {
            get {
                return ResourceManager.GetString("NotInChannel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The playlist {1} has been enqueued by {1}.
        /// </summary>
        internal static string PlaylistQueued {
            get {
                return ResourceManager.GetString("PlaylistQueued", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Downloading song....
        /// </summary>
        internal static string SongDownloading {
            get {
                return ResourceManager.GetString("SongDownloading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Now playing {0}, queued by {1}.
        /// </summary>
        internal static string SongPlaying {
            get {
                return ResourceManager.GetString("SongPlaying", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The song has been queued.
        /// </summary>
        internal static string SongQueued {
            get {
                return ResourceManager.GetString("SongQueued", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Skipping song {0}.
        /// </summary>
        internal static string SongSkipped {
            get {
                return ResourceManager.GetString("SongSkipped", resourceCulture);
            }
        }
    }
}