using System;
using R2API;
using RoR2;
using UnityEngine;

namespace ArtifactGroup
{
	// Token: 0x02000009 RID: 9
	public abstract class ArtifactBase
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600002E RID: 46
		public abstract string ArtifactName { get; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600002F RID: 47
		public abstract string ArtifactLangTokenName { get; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000030 RID: 48
		public abstract string ArtifactDescription { get; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000031 RID: 49
		public abstract Sprite ArtifactEnabledIcon { get; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000032 RID: 50
		public abstract Sprite ArtifactDisabledIcon { get; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00003B21 File Offset: 0x00001D21
		public bool ArtifactEnabled
		{
			get
			{
				return RunArtifactManager.instance.IsArtifactEnabled(this.ArtifactDef);
			}
		}

		// Token: 0x06000034 RID: 52
		public abstract void Init();

		// Token: 0x06000035 RID: 53 RVA: 0x00003B34 File Offset: 0x00001D34
		protected void CreateLang()
		{
			LanguageAPI.Add("ARTIFACT_" + this.ArtifactLangTokenName + "_NAME", this.ArtifactName);
			LanguageAPI.Add("ARTIFACT_" + this.ArtifactLangTokenName + "_DESCRIPTION", this.ArtifactDescription);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003B84 File Offset: 0x00001D84
		protected void CreateArtifact()
		{
			this.ArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
			this.ArtifactDef.cachedName = "ARTIFACT_" + this.ArtifactLangTokenName;
			this.ArtifactDef.nameToken = "ARTIFACT_" + this.ArtifactLangTokenName + "_NAME";
			this.ArtifactDef.descriptionToken = "ARTIFACT_" + this.ArtifactLangTokenName + "_DESCRIPTION";
			this.ArtifactDef.smallIconSelectedSprite = this.ArtifactEnabledIcon;
			this.ArtifactDef.smallIconDeselectedSprite = this.ArtifactDisabledIcon;
			ContentAddition.AddArtifactDef(this.ArtifactDef);
		}

		// Token: 0x06000037 RID: 55
		public abstract void Hooks();

		// Token: 0x04000044 RID: 68
		public ArtifactDef ArtifactDef;
	}
}
