using System;

using UnityEngine;

namespace AutoInority
{
    public class AutomatonBuf : UnitBuf
    {
        private readonly CreatureModel _creature;
        private GameObject _gameObject;

        public AutomatonBuf(CreatureModel creature) : base()
        {
            _creature = creature;

            duplicateType = BufDuplicateType.ONLY_ONE;
            remainTime = float.MaxValue;
            type = UnitBufType.ADD_SUPERARMOR;
        }

        public static Sprite GetPortrait(string portraitSrc) => Resources.Load<Sprite>(portraitSrc);

        public override void Init(UnitModel model)
        {
            base.Init(model);
            try
            {
                var portrait = GetPortrait(_creature.metaInfo.portraitSrc);
                var rect = new Rect(0f, 0f, portrait.texture.width, portrait.texture.height);
                var pivot = new Vector2(portrait.texture.width / 2, portrait.texture.width / 2);
                Sprite.Create(Add_On.duplicateTexture(portrait.texture), rect, pivot);

                var gameObject = new GameObject("unname");
                var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = portrait;
                gameObject.transform.SetParent((model as AgentModel).GetUnit().gameObject.transform);
                var width = spriteRenderer.sprite.texture.width;
                var num = 100f / width;

                gameObject.transform.localScale = new Vector3(num, num);
                gameObject.transform.localPosition = new Vector3(0f, 3.5f);
                gameObject.transform.localRotation = Quaternion.identity;
                _gameObject = gameObject;
                _gameObject.SetActive(value: true);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _gameObject.SetActive(value: false);
        }
    }
}