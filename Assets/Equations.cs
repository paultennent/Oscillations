using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equations : AbstractGameEffects {

	float gravity = 9.8f;

	float swingPeriod;
	float halfSwingPeriod;
	float max_sensor_G_reading_in_z_axis_as_crosses_mid_point; //don't have this as it isn't passed

	//predfined - these need correct measurements
	float swing_seat_radius = 1.5f;
	float phone_sensor_radius = 1.55f;
	float centre_of_gravity_radius = 1.25f; //depends on max_sensor_G_reading_in_z_axis_as_crosses_mid_point which we don't have - using approx for now
	float rider_eye_radius = 1f;

	//brendan's shopping list

	public Dictionary<string,float> shopping;



	//used to Calculate deltas
	float last_swing_angle_at_time_t = 0;
	float last_rider_CofG_x_coordinate_at_time_t = 0;
	float last_rider_CofG_z_coordinate_at_time_t = 0;
	float last_rider_eye_x_coordinate_at_time_t = 0;
	float last_rider_eye_z_coordinate_at_time_t = 0;


	// Use this for initialization
	void Start () {
		base.Start ();

		shopping = new Dictionary<string,float> ();

		shopping["swing_angle_at_time_t"] = 0;
		shopping["delta_swing_angle_at_time_t"] = 0;
		shopping["swing_angular_velocity_at_time_t"] = 0;
		shopping["rider_CofG_x_coordinate_at_time_t"] = 0;
		shopping["delta_rider_CofG_x_coordinate_at_time_t"] = 0;
		shopping["rider_CofG_z_coordinate_at_time_t"] = 0;
		shopping["delta_rider_CofG_z_coordinate_at_time_t"] = 0;
		shopping["rider_eye_x_coordinate_at_time_t"] = 0;
		shopping["delta_rider_eye_x_coordinate_at_time_t"] = 0;
		shopping["rider_eye_z_coordinate_at_time_t"] = 0;
		shopping["delta_rider_eye_z_coordinate_at_time_t"] = 0;
		shopping["rider_CofG_velocity_in_x"] = 0;
		shopping["rider_CofG_velocity_in_z"] = 0;
		shopping["rider_eye_velocity_in_x"] = 0; 
		shopping["rider_eye_velocity_in_z"] = 0;
		shopping["body_linear_speed"] = 0;
		shopping["eye_linear_speed"] = 0;
		shopping["centripetal_Force_per_unit_mass"] = 0;
		shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] = 0;
		shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"] = 0;
		shopping["resultant_force_per_unit_mass_at_CofG"] = 0;
		shopping["angle_as_offset_from_perpendicular_to_CofG_direction"] = 0;
		shopping["Centripetal_G_Force"] = 0;
		shopping["Gravity_G_Force_at_CofG_tangential_to_direction"] = 0;
		shopping["Gravity_G_Force_at_CofG_perpendicular_to_direction"] = 0;
		shopping["resultant_G_Force_at_CofG"] = 0;

	}
		
	
	// Update is called once per frame
	void Update () {
		base.Update ();
		swingPeriod = swingCycleTime;
		halfSwingPeriod = swingPeriod / 2f;

		shopping["swing_angle_at_time_t"] = swingAngle;
		shopping["delta_swing_angle_at_time_t"] = shopping["swing_angle_at_time_t"] - last_swing_angle_at_time_t;
		last_swing_angle_at_time_t = shopping["swing_angle_at_time_t"];

		shopping["swing_angular_velocity_at_time_t"] = shopping["delta_swing_angle_at_time_t"] / Time.deltaTime;

		shopping["rider_CofG_x_coordinate_at_time_t"] = centre_of_gravity_radius * Mathf.Sin (shopping["swing_angle_at_time_t"]);
		shopping["delta_rider_CofG_x_coordinate_at_time_t"] = shopping["rider_CofG_x_coordinate_at_time_t"] - last_rider_CofG_x_coordinate_at_time_t;
		last_rider_CofG_x_coordinate_at_time_t = shopping["rider_CofG_x_coordinate_at_time_t"];

		shopping["rider_CofG_z_coordinate_at_time_t"] = centre_of_gravity_radius * Mathf.Sin (shopping["swing_angle_at_time_t"]) * Mathf.Tan (shopping["swing_angle_at_time_t"]/2f);
		shopping["delta_rider_CofG_z_coordinate_at_time_t"] = shopping["rider_CofG_z_coordinate_at_time_t"] - last_rider_CofG_z_coordinate_at_time_t;
		last_rider_CofG_z_coordinate_at_time_t = shopping["rider_CofG_z_coordinate_at_time_t"];

		shopping["rider_eye_x_coordinate_at_time_t"] = rider_eye_radius * Mathf.Sin (shopping["swing_angle_at_time_t"]);
		shopping["delta_rider_eye_x_coordinate_at_time_t"] = shopping["rider_eye_x_coordinate_at_time_t"] - last_rider_eye_x_coordinate_at_time_t;
		last_rider_eye_x_coordinate_at_time_t = shopping["rider_eye_x_coordinate_at_time_t"];

		shopping["rider_eye_z_coordinate_at_time_t"] = 2f * rider_eye_radius * (Mathf.Sin (shopping["swing_angle_at_time_t"]) * Mathf.Sin (shopping["swing_angle_at_time_t"]));
		shopping["delta_rider_eye_z_coordinate_at_time_t"] = shopping["rider_eye_z_coordinate_at_time_t"] - last_rider_eye_z_coordinate_at_time_t;
		last_rider_eye_z_coordinate_at_time_t = shopping["rider_eye_z_coordinate_at_time_t"];

		shopping["rider_CofG_velocity_in_x"] = shopping["delta_rider_CofG_x_coordinate_at_time_t"] / Time.deltaTime;
		shopping["rider_CofG_velocity_in_z"] = shopping["delta_rider_CofG_z_coordinate_at_time_t"] / Time.deltaTime;
		shopping["rider_eye_velocity_in_x"] = shopping["delta_rider_eye_x_coordinate_at_time_t"] / Time.deltaTime;
		shopping["rider_eye_velocity_in_z"] = shopping["delta_rider_eye_z_coordinate_at_time_t"] / Time.deltaTime;

		shopping["body_linear_speed"] = shopping["swing_angular_velocity_at_time_t"] * centre_of_gravity_radius;
		shopping["eye_linear_speed"] = shopping["swing_angular_velocity_at_time_t"] * rider_eye_radius;

		shopping["centripetal_Force_per_unit_mass"] = centre_of_gravity_radius * (shopping["swing_angular_velocity_at_time_t"] * shopping["swing_angular_velocity_at_time_t"]);
		shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] = gravity * Mathf.Sin (shopping["swing_angle_at_time_t"]);
		shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"] = gravity * Mathf.Cos (shopping["swing_angle_at_time_t"]);

		shopping["resultant_force_per_unit_mass_at_CofG"] = Mathf.Sqrt(((shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"]) * (shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"])) + (shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] * shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"]));

		shopping["angle_as_offset_from_perpendicular_to_CofG_direction"] = shopping["swing_angle_at_time_t"] - Mathf.Atan ((shopping["centripetal_Force_per_unit_mass"] * Mathf.Sin (shopping["swing_angle_at_time_t"])) / ((shopping["centripetal_Force_per_unit_mass"] * Mathf.Cos (shopping["swing_angle_at_time_t"])) + shopping["resultant_force_per_unit_mass_at_CofG"]));

		shopping["Centripetal_G_Force"] = (centre_of_gravity_radius / gravity) * (shopping["swing_angular_velocity_at_time_t"] * shopping["swing_angular_velocity_at_time_t"]);
		shopping["Gravity_G_Force_at_CofG_tangential_to_direction"] = Mathf.Cos (shopping["swing_angle_at_time_t"]);
		shopping["Gravity_G_Force_at_CofG_perpendicular_to_direction"] = Mathf.Sin (shopping["swing_angle_at_time_t"]);

		shopping["resultant_G_Force_at_CofG"] = (1f / gravity) * (Mathf.Sqrt ((shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"]) * (shopping["centripetal_Force_per_unit_mass"] + shopping["gravity_force_per_unit_mass_at_CofG_perpendicular_to_direction"])) + (shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"] * shopping["gravity_force_per_unit_mass_at_CofG_tangential_to_direction"]));
	}
}
