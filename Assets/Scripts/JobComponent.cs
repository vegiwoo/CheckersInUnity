using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

public class JobComponent : MonoBehaviour
{
	private int _frames;
	private bool _plus;

	private JobCalc _jobCalc;
	private JobHandle _jobCalcHandle;
	private AdditionalJob _addJob;
	private JobHandle _addJobCalcHandle;
	private NativeArray<Vector3> _velocities;
	private NativeArray<Vector3> _positions;

	public Vector3 Velocity { get; set; }

	private void Start()
	{
		//Задаем начальное перемещение
		Velocity = Random.insideUnitSphere;
		//Задаем начальное положение
		transform.position = Random.insideUnitSphere * Random.Range(-100f, 100f);

		//Заглушка, для JobSystem
		_velocities = new NativeArray<Vector3>(new Vector3[0], Allocator.TempJob);
		_jobCalc = new JobCalc
		{
			Vectors = _velocities,
			Delta = Time.deltaTime,
			Velocity = Velocity
		};
		_jobCalcHandle = _jobCalc.Schedule();
		_positions = new NativeArray<Vector3>(new Vector3[0], Allocator.TempJob);
	}
	/*
	#region
	void Update()
    {
		//Получаем ссылки на все ресурсы
		var collection = CreaterManager.Collection;

		//Перемещение объекта
        if(CreaterManager.CurrentUsingJobSystemTransform)
		{
			//Заканчиваем выполнение предыдущей задачи
			_jobCalcHandle.Complete();
			//Очищаем и обрабатываем результаты
			Velocity = _jobCalc.Velocity;
			_velocities.Dispose();
			//Новая задача
			_velocities = new NativeArray<Vector3>(collection.Select(t => t.Velocity).ToArray(), Allocator.TempJob);
			_jobCalc = new JobCalc
			{
				Vectors = _velocities,
				Delta = Time.deltaTime,
				Velocity = Velocity
			};
			_jobCalcHandle = _jobCalc.Schedule();
		}
		else
		{
			Vector3 velocity = Vector3.zero;
			foreach(var component in collection)
			{
				velocity += component.Velocity;
				velocity = velocity.normalized;
			}

			Velocity = (Velocity + velocity * Time.deltaTime).normalized;
		}

		//Каждые 50 кадров сферы начинают перемещаться в противоположную сторону
		if(_frames == 50)
		{
			_frames = 0;
			_plus = !_plus;
		}
		_frames++;
		transform.position = _plus
			? transform.position + Velocity
			: transform.position - Velocity;

		//Дополнительные вычисления
		if (!CreaterManager.CurrentAdditionalCalculations) return;

		if (CreaterManager.CurrentUsingJobSystemAdditionalCalc) AdditionalCalculationsWithJobs(collection.Select(t => t.transform.position));
		else AdditionalCalculations(collection.Select(t => t.transform.position));
	}
	#endregion
	*/
	//Нагрузочные вычисления
	private void AdditionalCalculations(IEnumerable<Vector3> positions)
	{
		float weight;
		Vector3 selfPos = transform.position;
		weight = 0f;
		//Вычисление расстояний между всеми игровыми объектами
		foreach (var pos in positions)
		{
			weight += Vector3.Distance(pos, selfPos);
		}
	}

	private void AdditionalCalculationsWithJobs(IEnumerable<Vector3> positions)
	{
		//Заканчиваем выполнение предыдущей задачи
		_addJobCalcHandle.Complete();
		_positions.Dispose();

		//Новая задача
		_positions = new NativeArray<Vector3>(positions.ToArray(), Allocator.TempJob);
		_addJob = new AdditionalJob
		{
			Positions = _positions,
			SelfPosition = transform.position
		};
		_addJobCalcHandle = _addJob.Schedule();
	}

	private struct JobCalc : IJob
	{
		[ReadOnly]
		public NativeArray<Vector3> Vectors;
		[ReadOnly]
		public float Delta;

		public Vector3 Velocity;

		public void Execute()
		{
			Vector3 velocity = Vector3.zero;
			foreach (var vector in Vectors)
			{
				velocity += vector;
				velocity = velocity.normalized;
			}

			Velocity = (Velocity + velocity * Delta).normalized;
		}
	}

	private struct AdditionalJob : IJob
	{
		[ReadOnly]
		public NativeArray<Vector3> Positions;
		[ReadOnly]
		public Vector3 SelfPosition;


		public void Execute()
		{
			float weight = 0f;
			//Вычисление расстояний между всеми игровыми объектами
			foreach (var pos in Positions)
			{
				weight += Vector3.Distance(pos, SelfPosition);
			}
		}
	}
}
