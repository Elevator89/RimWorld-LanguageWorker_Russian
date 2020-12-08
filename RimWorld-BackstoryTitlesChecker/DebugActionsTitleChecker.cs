using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace BackstoryTitlesChecker
{
	public static class DebugActionsTitleChecker
	{
		[DebugAction("Translation", null, allowedGameStates = AllowedGameStates.Entry)]
		private static void CheckTitleLengths()
		{
			Backstory[] backstories = BackstoryDatabase.allBackstories.Values.ToArray();

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;

			float maxWidth = 160f;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Oversized backstory titles (>{maxWidth}px):");

			foreach (Backstory backstory in backstories)
			{
				CheckAndLogSize(backstory.identifier + ".title", backstory.title.CapitalizeFirst());
				CheckAndLogSize(backstory.identifier + ".titleFemale", backstory.titleFemale.CapitalizeFirst());
			}

			Log.Message(sb.ToString());

			void CheckAndLogSize(string id, string title)
			{
				Vector2 size = Text.CalcSize(title);
				if (size.x > maxWidth)
					sb.AppendLine($"{id}: {title} - {size.x}px");
			}
		}
	}
}
