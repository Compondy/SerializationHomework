using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(PositionSaver))]
    public class ReplayMover : MonoBehaviour
    {
        private PositionSaver _save;

        private int _index;
        private PositionSaver.Data _prev;
        private float _duration;

        private void Start()
        {
            //todo comment: зачем нужны эти проверки?
            //Проверка на наличие требуемого компонента PositionSaver. Если компонента нет или в нем нет записей, то дальше выполняется код условия.
            if (!TryGetComponent(out _save) || _save.Records.Count == 0)
            {
                Debug.LogError("Records incorrect value", this);
                //todo comment: Для чего выключается этот компонент?
                //Нет смысла воспроизводить запись, если ее нет. После выключения не будет выполняться Update.
                enabled = false;
            }
        }

        private void Update()
        {
            var curr = _save.Records[_index];
            //todo comment: Что проверяет это условие (с какой целью)? 
            //Если время в начале кадра больше сохраненного времени в записи начиная с первой записи (по индексу 0), то выполняется код условия. Проверка с целью дождаться момента, когда была сохранена позиция при записи.
            if (Time.time > curr.Time)
            {
                _prev = curr;
                _index++;
                //todo comment: Для чего нужна эта проверка?
                //На последней записи индекс выйдет за пределы диапазона (станет равным _save.Records.Count) и при следующем Update произойдет исключение в _save.Records[_index]. Чтобы это избежать делается проверка и выключается скрипт.
                if (_index >= _save.Records.Count)
                {
                    enabled = false;
                    Debug.Log($"<b>{name}</b> finished", this);
                }
            }
            //todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
            //Вычисляется коэффициент (множитель), который дальше используется для линейной "интерполяции" позиции объекта между двумя точками.
            var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
            //todo comment: Зачем нужна эта проверка?
            //Тип float может принимать здание NaN (деление 0 на 0 или одна из переменных NaN). Если это произойдет, то результат Lerp также будет NaN, что приведет к ошибке.
            if (float.IsNaN(delta) || float.IsInfinity(delta) || float.IsNegativeInfinity(delta)) delta = 0f;
            //todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
            //_prev.Position + (curr.Position - _prev.Position) * delta. delta у нас увеличивается от 0 до 1. Таким образом определяются линейно точки между _prev и curr на основе прошедшего времени.
            transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
        }
    }
}