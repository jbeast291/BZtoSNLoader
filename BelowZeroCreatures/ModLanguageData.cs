using System.Collections.Generic;

namespace BelowZeroCreatures;

public static class ModLanguageData
{
    public static readonly string[] MiscLocalizationToRequest =
    {
        "TitanHolefishOxygenUse",
        "WhaleInteract"
    };
    
    public static readonly Dictionary<string, string> EnglishNameReplacements = new()
    {
        { "ArcticRay", "Shimmer ray"},
        { "Ency_ArcticRay", "Shimmer Ray"},
        { "ArcticPeeper", "Leucistic peeper"},
        { "CookedArcticPeeper", "Cooked leucistic peeper"},
        { "CuredArcticPeeper", "Cured leucistic peeper"},
        { "Ency_ArcticPeeper", "Leucistic Peeper"}
    };
    
    public static readonly Dictionary<string, PdaPhraseReplacement[]> PdaEntryModifications = new()
    {
        { "EncyDesc_ArcticRay", new[] {new PdaPhraseReplacement("low temperature", "various")}},
        { "EncyDesc_ArcticPeeper", new[]
        {
            new PdaPhraseReplacement("camouflaged against the ice", "adapted for deep sea conditions"),
            new PdaPhraseReplacement("Researcher notes: 'Don't like the auto-generated name. Suggest: Iceclops.'",
                "Note: 'Features an unusual number of adaptations seen in cold-water fish. Alternative name: Arctic Peeper.'"),
        }},
        { "EncyDesc_Brinewing", new[]
        {
            new PdaPhraseReplacement("supercooled salt water", "regurgitated brine"),
            new PdaPhraseReplacement("sea water as", "sea water and brine as"),
            new PdaPhraseReplacement("freeze its prey", "incapacitate its prey"),
            new PdaPhraseReplacement("the supercooled brine and freezing the seawater", "the concentrated brine and chemically burning its target"),
            new PdaPhraseReplacement("warm-blooded lifeforms", "prey fish"),
            new PdaPhraseReplacement("cold enough to freeze most vertebrates", "corrosive enough to harm lifeforms that are native to the cave system"),
            new PdaPhraseReplacement("act as radiators, releasing heat and cooling the bladder's contents", "are covered in pores that expel excess unsalinated water back into the environment"),
            new PdaPhraseReplacement("The real danger is being helplessly encased in a block of ice, sinking ever closer towards the brinewing's jaws", "Regardless, it effectively wards off whatever the brinewing perceives as a threat, including you")
        }}
    };
    
    public class PdaPhraseReplacement
    {
        public string OriginalPhrase { get; }
        public string ReplacementPhrase { get; }

        public PdaPhraseReplacement(string originalPhrase, string replacementPhrase)
        {
            OriginalPhrase = originalPhrase;
            ReplacementPhrase = replacementPhrase;
        }
    }
}