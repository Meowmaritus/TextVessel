using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextVessel.Credits;

namespace TextVessel
{
    public class AboutDataContext
    {
        public ObservableCollection<CreditDef> Credits { get; set; } = new ObservableCollection<CreditDef>
        {
            new CreditDef()
            {
                Purpose = "Dark Souls text editor",
                ProjectDispName = "Text Vessel",
                ProjectInternalName = "TextVessel",
                ProjectURL = "https://github.com/Meowmaritus/TextVessel",
                Creator = "Meowmaritus",
                LicenseName = "MIT",
                EmbeddedLicenseFilename = "TextVessel.License.TextVessel.LICENSE.html",
            },
            new CreditDef()
            {
                Purpose = "Dark Souls data file loading/manipulation/saving",
                ProjectDispName = "MeowDSIO",
                ProjectInternalName = "MeowDSIO",
                ProjectURL = "https://github.com/Meowmaritus/MeowDSIO",
                Creator = "Meowmaritus",
                LicenseName = "MIT",
                EmbeddedLicenseFilename = "TextVessel.License.MeowDSIO.LICENSE.html",
            },
            new CreditDef()
            {
                Purpose = "Sleek GUI styling",
                ProjectDispName = "Visual Studio 2012 Metro Styles for WPF",
                ProjectInternalName = "Selen.Wpf",
                ProjectURL = "https://www.codeproject.com/Articles/442856/Visual-Studio-Metro-Styles-for-WPF",
                Creator = "Winfried Lötzsch",
                LicenseName = "CPOL",
                EmbeddedLicenseFilename = "TextVessel.License.Selen.Wpf.LICENSE.html",
            },
            new CreditDef()
            {
                Purpose = "High-performance editor textbox with undo/redo and syntax highlighting for the <?specialTags?> in Dark Souls.",
                ProjectDispName = "AvalonEdit",
                ProjectInternalName = "ICSharpCode.AvalonEdit",
                ProjectURL = "https://github.com/icsharpcode/AvalonEdit",
                Creator = "ICSharpCode",
                LicenseName = "MIT",
                EmbeddedLicenseFilename = "TextVessel.License.AvalonEdit.LICENSE.html",
            },
            new CreditDef()
            {
                Purpose = "JSON serialization for 'TextVessel_UserConfig.json'",
                ProjectDispName = "Json.NET",
                ProjectInternalName = "Newtonsoft.Json",
                ProjectURL = "https://www.newtonsoft.com/json",
                Creator = "Newtonsoft",
                LicenseName = "MIT",
                EmbeddedLicenseFilename = "TextVessel.License.Newtonsoft.Json.LICENSE.html",
            },
            new CreditDef()
            {
                Purpose = "Embedding of the 10+ DLLs referenced by TextVessel into the 'TextVessel.exe' assembly",
                ProjectDispName = "Costura add-in for Fody",
                ProjectInternalName = "Costura.Fody",
                ProjectURL = "https://github.com/Fody/Costura",
                Creator = "Simon Cropp & additional contributors",
                LicenseName = "MIT",
                EmbeddedLicenseFilename = "TextVessel.License.Costura.Fody.LICENSE.html",
            },
        };
    }
}
