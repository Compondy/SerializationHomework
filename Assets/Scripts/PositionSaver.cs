using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


namespace DefaultNamespace
{
    public class PositionSaver : MonoBehaviour
	{
		[Serializable]
		public struct Data
		{
			public Vector3 Position;
			public float Time;
		}

		[ReadOnly]
        [Tooltip("Use context menu \"Create File\" to set this field.")]
        public TextAsset _json;

		[HideInInspector]
		public List<Data> Records { get => _records; private set => _records = value; }
        
        [SerializeField]
        private List<Data> _records; //с get/set не сериализуется (сериализуется instanceid), поэтому используется резервное поле без них. Источник: https://stackoverflow.com/questions/41787091/unity-c-sharp-jsonutility-is-not-serializing-a-list

        private void Awake()
		{
            //todo comment: Что будет, если в теле этого условия не сделать выход из метода?
            //Продолжится выполнение после кода условия и произойдет исключение на команде "JsonUtility.FromJsonOverwrite(_json.text, this);", т.к. _json.text будет null
            if (_json == null)
			{
				gameObject.SetActive(false);
				Debug.LogError("Please, create TextAsset and add in field _json");
				return;
			}

			JsonUtility.FromJsonOverwrite(_json.text, this);
			//todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
			//Records инициализируется новым списком если он не был инициализирован. Проверка позволяет избежать замены уже существующего списка Records на пустой.
			if (Records == null)
				Records = new List<Data>(10);
		}

		private void OnDrawGizmos()
		{
			//todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
			//Проверка на наличие Records и записи в ней. Если проверку не сделать, то будет исключение при попытке получить первую запись в data[0]
			if (Records == null || Records.Count == 0) return;
			var data = Records;
			var prev = data[0].Position;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(prev, 0.3f);
			//todo comment: Почему итерация начинается не с нулевого элемента?
			//Потому, что нулевой уже взят в переменную prev. Т.е. нулевая итерация пропускается, т.к. рисовать линию от точки к ней самой смысла нет.
			//А сферы рисуются только на последующих точках.
			for (int i = 1; i < data.Count; i++)
			{
				var curr = data[i].Position;
				Gizmos.DrawWireSphere(curr, 0.3f);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}
		
#if UNITY_EDITOR
		[ContextMenu("Create File")]
		private void CreateFile()
		{
			//todo comment: Что происходит в этой строке?
			//Создается поток, который можно использовать для чтения/записи в новый файл. В качестве переменной передается строка пути к файлу, которая создается из здачения dataPath и названия файла Path.txt 
			var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
			//todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав) 
			//Освобождает поток (вызывает соответствующий код Dispose, который в данном случае закрывает файл и освобождает связанные объекты).
			stream.Dispose();
			UnityEditor.AssetDatabase.Refresh();
			//В Unity можно искать объекты по их типу, для этого используется префикс "t:"
			//После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
			var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
			foreach (var guid in guids)
			{
				//Этой командой можно получить путь к ассету через его гуид
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				//Этой командой можно загрузить сам ассет
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				//todo comment: Для чего нужны эти проверки?
				//Если asset не null, т.е. найден и его имя Path (точно тот найден). Хотя вторая проверка излишняя - guid и так уникально определяет ассет.
				if(asset != null && asset.name == "Path")
				{
					_json = asset;
					UnityEditor.EditorUtility.SetDirty(this);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
					//todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
					//Нет смысла дальше искать ассет, если он уже найден. Другой с таким же guid не найдется, т.к. guid уникален.
					return;
				}
			}
		}

		private void OnDestroy()
		{
			string json = JsonUtility.ToJson(this);
			File.WriteAllText(Path.Combine(Application.dataPath, "Path.txt"), json);
        }
#endif
	}
}