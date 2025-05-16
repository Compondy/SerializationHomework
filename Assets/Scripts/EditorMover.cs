using UnityEngine;

namespace DefaultNamespace
{

    [RequireComponent(typeof(PositionSaver))]
    public class EditorMover : MonoBehaviour
    {
        private PositionSaver _save;
        private float _currentDelay;

        //todo comment: Что произойдёт, если _delay > _duration?
        //_delay это промежуток, чере который сохранять позицию, _duration это все время в течение которого сохранять позицию. Если _delay будет больше _duration то позиция ни разу не сохранится.
        [Range(0.2f, 1.0f)]
        private float _delay = 0.5f;
        [Min(0.2f)]
        private float _duration = 10f;

        private void Start()
        {
            //todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
            //Start выполняется только 1 раз, а Update каждый кадр. Осуществлять поиск одного и того же объекта много раз бессмысленно и это ухудшит производительность.
            _save = GetComponent<PositionSaver>();
            _save.Records.Clear();

            if (_duration <= _delay) _duration = _delay * 5;
        }

        private void Update()
        {
            _duration -= Time.deltaTime;
            if (_duration <= 0f)
            {
                enabled = false;
                Debug.Log($"<b>{name}</b> finished", this);
                return;
            }

            //todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
            //_duration уменьшается на время между кадрами для того, чтобы счетчик времени _duration истек за соответственно прошеднее время. Если также уменьшать _delay то _delay станет меньше нуля за 0,5 секунд и дальше продолжит быть меньше 0 и позиция будет сохраняться каждый кадр.
            _currentDelay -= Time.deltaTime;
            if (_currentDelay <= 0f)
            {
                _currentDelay = _delay;
                _save.Records.Add(new PositionSaver.Data
                {
                    Position = transform.position,
                    //todo comment: Для чего сохраняется значение игрового времени?
                    //Чтобы можно было восстановить последовательность сохраненных позиций объекта.
                    Time = Time.time,
                });
            }
        }
    }
}