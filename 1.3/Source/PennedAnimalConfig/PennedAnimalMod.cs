using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace PennedAnimalConfig
{
    public class PennedAnimalMod : Mod
    {
        public static PennedAnimalMod mod;
        public static PennedAnimalSettings settings;
		public static Vector2 scrollPosition = Vector2.zero;

        public override string SettingsCategory() => "Penned Animal Config";

        public PennedAnimalMod(ModContentPack content) : base(content)
        {
            mod = this;
            settings = GetSettings<PennedAnimalSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
			List<string> list = (from x in settings.animalDictionary.Keys.ToList() orderby x descending select x).ToList();
			Listing_Standard listing_Standard = new Listing_Standard();
			Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
			Rect rect = new Rect(0f, 0f, inRect.width - 30f, (float)((list.Count / 2 + 2) * 24));
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect, true);
			listing_Standard.ColumnWidth = rect.width / 2.2f;
			listing_Standard.Begin(rect);
			for (int i = list.Count - 1; i >= 0; i--)
            {
                if (i == (list.Count / 2) + 1)
                {
                    listing_Standard.NewColumn();
                }

                ThingDef animal = DefDatabase<ThingDef>.GetNamedSilentFail(list[i]);
                if (animal != null)
                {
                    bool value = settings.animalDictionary[list[i]];
                    listing_Standard.CheckboxLabeled(animal.LabelCap, ref value, null);
                    settings.animalDictionary[list[i]] = value;
                }
            }
			listing_Standard.End();
			Widgets.EndScrollView();

            PennedAnimalStartup.SetPennedAnimals(settings);
		}
    }

    public class PennedAnimalSettings : ModSettings
    {
        public Dictionary<string, bool> animalDictionary = new Dictionary<string, bool>();

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref animalDictionary, "animalDictionary");
        }
    }

    [StaticConstructorOnStartup]
    public static class PennedAnimalStartup
    {
        static PennedAnimalStartup()
        {
            PennedAnimalSettings settings = PennedAnimalMod.settings;

            FindNewAnimals(settings);
            SetPennedAnimals(settings);
        }

        public static void FindNewAnimals(PennedAnimalSettings settings)
        {
            List<ThingDef> list = (from x in DefDatabase<ThingDef>.AllDefs where (bool)(x.race?.Animal ?? false) select x).ToList();
            foreach (ThingDef def in list)
            {
                if (!settings.animalDictionary.ContainsKey(def.defName))
                {
                    bool penned = def.race.roamMtbDays > 0;
                    settings.animalDictionary.Add(def.defName, penned);
                }
            }
        }

        public static void SetPennedAnimals(PennedAnimalSettings settings)
        {
            foreach (KeyValuePair<string, bool> pair in settings.animalDictionary)
            {
                ThingDef animal = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
                if(animal != null)
                {
                    if (pair.Value)
                    {
                        animal.race.roamMtbDays = 2;
                    }
                    else
                    {
                        animal.race.roamMtbDays = null;
                    }
                }
            }
        }
    }
}
