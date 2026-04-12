using UnityEngine;

namespace SDUI.UI
{
	public class SpinnerView : MonoBehaviour
	{
		[SerializeField]
		private float _interval = 0.1f;
		[SerializeField]
		private float _speed = 360f;

		private float _timer;

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer < _interval)
				return;

			_timer -= _interval;
			transform.Rotate(0f, 0f, -_speed * Time.deltaTime);
		}
	}
}