using System;
using System.IO;
using UnityEngine;

namespace AutoInority
{
	public class MacroBurf : UnitBuf
	{
		private GameObject GO;

		private CreatureModel who;

		public MacroBurf(CreatureModel model)
		{
			remainTime = 1000000f;
			who = model;
			type = UnitBufType.ADD_SUPERARMOR;
			duplicateType = BufDuplicateType.ONLY_ONE;
		}

		public override void Init(UnitModel model)
		{
			base.Init(model);
			try
			{
				Sprite portrait = GetPortrait(who.metaInfo.portraitSrc);
				Sprite.Create(Add_On.duplicateTexture(portrait.texture), new Rect(0f, 0f, portrait.texture.width, portrait.texture.height), new Vector2(portrait.texture.width / 2, portrait.texture.width / 2));
				GameObject gameObject = new GameObject("unname");
				SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = portrait;
				gameObject.transform.SetParent((model as AgentModel).GetUnit().gameObject.transform);
				int width = spriteRenderer.sprite.texture.width;
				float num = 100f / width;
				gameObject.transform.localScale = new Vector3(num, num);
				gameObject.transform.localPosition = new Vector3(0f, 3.5f);
				gameObject.transform.localRotation = Quaternion.identity;
				GO = gameObject;
				GO.SetActive(value: true);
			}
			catch (Exception)
			{
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			GO.SetActive(value: false);
		}

		public static Sprite GetPortrait(string portraitSrc)
		{
			string[] array = portraitSrc.Split('/');
			Sprite result = null;
			if (array[0] == "Custom")
			{
				Sprite sprite = Resources.Load<Sprite>("Sprites/Unit/creature/AuthorNote");
				{
					foreach (DirectoryInfo dir in Add_On.instance.DirList)
					{
						string path = dir.FullName + "/Creature/Portrait/" + array[1] + ".png";
						if (File.Exists(path))
						{
							byte[] data = File.ReadAllBytes(path);
							new Texture2D(2, 2).LoadImage(data);
							sprite.texture.LoadImage(data);
							result = sprite;
						}
					}
					return result;
				}
			}
			return Resources.Load<Sprite>(portraitSrc);
		}
	}
}
