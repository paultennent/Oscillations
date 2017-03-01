using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquationCharter : MonoBehaviour {

	public SimpleGraph graph;
	private Equations eq;

	private CircularBuffer<float> points;
	public int resolution = 256;

	public enum options // your custom enumeration
	{
		swing_angle_at_time_t,
		delta_swing_angle_at_time_t,
		swing_angular_velocity_at_time_t,
		rider_CofG_x_coordinate_at_time_t,
		delta_rider_CofG_x_coordinate_at_time_t,
		rider_CofG_z_coordinate_at_time_t,
		delta_rider_CofG_z_coordinate_at_time_t,
		rider_eye_x_coordinate_at_time_t,
		delta_rider_eye_x_coordinate_at_time_t,
		rider_eye_z_coordinate_at_time_t,
		delta_rider_eye_z_coordinate_at_time_t,
		rider_CofG_velocity_in_x,
		rider_CofG_velocity_in_z,
		rider_eye_velocity_in_x,
		rider_eye_velocity_in_z,
		body_linear_speed,
		eye_linear_speed,
		centripetal_Force_per_unit_mass,
		gravity_force_per_unit_mass_at_CofG_tangential_to_direction,
		gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction,
		resultant_force_per_unit_mass_at_CofG,
		angle_as_offset_from_perpendicular_to_CofG_direction,
		Centripetal_G_Force,
		Gravity_G_Force_at_CofG_tangential_to_direction,
		Gravity_G_Force_at_CofG_perpendicular_to_direction,
		resultant_G_Force_at_CofG
	};
	public EquationCharter.options dropDown = options.swing_angle_at_time_t;

	void Start(){
		eq = GameObject.Find("Controller").GetComponent<Equations> ();
		points = new CircularBuffer<float> (256);
	}

	// Update is called once per frame
	void Update () {
		addData (eq.shopping[dropDown.ToString()]);
		graph.setLabel(dropDown.ToString ());
	}

	private void addData(float d){
		points.Add (d);
		graph.SetPoints (points.ToArray ());
	}

}

public class CircularBuffer<T>
{
	Queue<T> _queue;
	int _size;

	public CircularBuffer(int size)
	{
		_queue = new Queue<T>(size);
		_size = size;
	}

	public void Add(T obj)
	{
		if (_queue.Count == _size)
		{
			_queue.Dequeue();
			_queue.Enqueue(obj);
		}
		else
			_queue.Enqueue(obj);
	}
	public T Read()
	{
		return _queue.Dequeue();
	}

	public T Peek()
	{
		return _queue.Peek();
	}

	public T[] ToArray(){
		return _queue.ToArray ();
	}
}
