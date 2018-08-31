using System;
using UnityEngine;

[AddComponentMenu("Relief Terrain/Helpers/Mouse Orbit - Dynamic Distance")]
public class RTP_MouseOrbit_DynamicDistance : MonoBehaviour
{
	private void Start()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		this.x = eulerAngles.y;
		this.y = eulerAngles.x;
		this.Reset();
	}

	public void DisableSteering(bool state)
	{
		this.disableSteering = state;
	}

	public void Reset()
	{
		this.lastLMBState = Input.GetMouseButton(0);
		this.disableSteering = false;
		this.cur_distance = this.distance;
		this.cur_xSpeed = 0f;
		this.cur_ySpeed = 0f;
		this.req_xSpeed = 0f;
		this.req_ySpeed = 0f;
		this.surfaceColliders = null;
		this.cur_ObjxSpeed = 0f;
		this.cur_ObjySpeed = 0f;
		this.req_ObjxSpeed = 0f;
		this.req_ObjySpeed = 0f;
		if (this.target)
		{
			Renderer[] componentsInChildren = this.target.GetComponentsInChildren<Renderer>();
			Bounds bounds = default(Bounds);
			bool flag = false;
			foreach (Renderer renderer in componentsInChildren)
			{
				if (!flag)
				{
					flag = true;
					bounds = renderer.bounds;
				}
				else
				{
					bounds.Encapsulate(renderer.bounds);
				}
			}
			Vector3 size = bounds.size;
			float num = (size.x <= size.y) ? size.y : size.x;
			num = ((size.z <= num) ? num : size.z);
			this.bounds_MaxSize = num;
			this.cur_distance += this.bounds_MaxSize * 1.2f;
			this.surfaceColliders = this.target.GetComponentsInChildren<Collider>();
		}
	}

	private void LateUpdate()
	{
		if (this.target && this.targetFocus)
		{
			if (!this.lastLMBState && Input.GetMouseButton(0))
			{
				this.DraggingObject = false;
				if (this.surfaceColliders != null)
				{
					RaycastHit raycastHit = default(RaycastHit);
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					foreach (Collider collider in this.surfaceColliders)
					{
						if (collider.Raycast(ray, out raycastHit, float.PositiveInfinity))
						{
							this.DraggingObject = true;
							break;
						}
					}
				}
			}
			else if (this.lastLMBState && !Input.GetMouseButton(0))
			{
				this.DraggingObject = false;
			}
			this.lastLMBState = Input.GetMouseButton(0);
			if (this.DraggingObject)
			{
				if (Input.GetMouseButton(0) && !this.disableSteering)
				{
					this.req_ObjxSpeed += (Input.GetAxis("Mouse X") * this.xObjSpeed * 0.02f - this.req_ObjxSpeed) * Time.deltaTime * 10f;
					this.req_ObjySpeed += (Input.GetAxis("Mouse Y") * this.yObjSpeed * 0.02f - this.req_ObjySpeed) * Time.deltaTime * 10f;
				}
				else
				{
					this.req_ObjxSpeed += (0f - this.req_ObjxSpeed) * Time.deltaTime * 4f;
					this.req_ObjySpeed += (0f - this.req_ObjySpeed) * Time.deltaTime * 4f;
				}
				this.req_xSpeed += (0f - this.req_xSpeed) * Time.deltaTime * 4f;
				this.req_ySpeed += (0f - this.req_ySpeed) * Time.deltaTime * 4f;
			}
			else
			{
				if (Input.GetMouseButton(0) && !this.disableSteering)
				{
					this.req_xSpeed += (Input.GetAxis("Mouse X") * this.xSpeed * 0.02f - this.req_xSpeed) * Time.deltaTime * 10f;
					this.req_ySpeed += (Input.GetAxis("Mouse Y") * this.ySpeed * 0.02f - this.req_ySpeed) * Time.deltaTime * 10f;
				}
				else
				{
					this.req_xSpeed += (0f - this.req_xSpeed) * Time.deltaTime * 4f;
					this.req_ySpeed += (0f - this.req_ySpeed) * Time.deltaTime * 4f;
				}
				this.req_ObjxSpeed += (0f - this.req_ObjxSpeed) * Time.deltaTime * 4f;
				this.req_ObjySpeed += (0f - this.req_ObjySpeed) * Time.deltaTime * 4f;
			}
			this.distance -= Input.GetAxis("Mouse ScrollWheel") * this.ZoomWheelSpeed;
			this.distance = Mathf.Clamp(this.distance, this.minDistance, this.maxDistance);
			this.cur_ObjxSpeed += (this.req_ObjxSpeed - this.cur_ObjxSpeed) * Time.deltaTime * 20f;
			this.cur_ObjySpeed += (this.req_ObjySpeed - this.cur_ObjySpeed) * Time.deltaTime * 20f;
			this.target.transform.RotateAround(this.targetFocus.position, Vector3.Cross(this.targetFocus.position - base.transform.position, base.transform.right), -this.cur_ObjxSpeed);
			this.target.transform.RotateAround(this.targetFocus.position, Vector3.Cross(this.targetFocus.position - base.transform.position, base.transform.up), -this.cur_ObjySpeed);
			this.cur_xSpeed += (this.req_xSpeed - this.cur_xSpeed) * Time.deltaTime * 20f;
			this.cur_ySpeed += (this.req_ySpeed - this.cur_ySpeed) * Time.deltaTime * 20f;
			this.x += this.cur_xSpeed;
			this.y -= this.cur_ySpeed;
			this.y = RTP_MouseOrbit_DynamicDistance.ClampAngle(this.y, this.yMinLimit + this.normal_angle, this.yMaxLimit + this.normal_angle);
			if (this.surfaceColliders != null)
			{
				RaycastHit raycastHit2 = default(RaycastHit);
				Vector3 vector = Vector3.Normalize(this.targetFocus.position - base.transform.position);
				float num = 0.01f;
				bool flag = false;
				foreach (Collider collider2 in this.surfaceColliders)
				{
					if (collider2.Raycast(new Ray(base.transform.position - vector * this.bounds_MaxSize, vector), out raycastHit2, float.PositiveInfinity))
					{
						num = Mathf.Max(Vector3.Distance(raycastHit2.point, this.targetFocus.position) + this.distance, num);
						flag = true;
					}
				}
				if (flag)
				{
					this.cur_distance += (num - this.cur_distance) * Time.deltaTime * 4f;
				}
			}
			Quaternion rotation = Quaternion.Euler(this.y, this.x, 0f);
			Vector3 position = rotation * new Vector3(0f, 0f, -this.cur_distance) + this.targetFocus.position;
			base.transform.rotation = rotation;
			base.transform.position = position;
		}
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public void set_normal_angle(float a)
	{
		this.normal_angle = a;
	}

	public GameObject target;

	public Transform targetFocus;

	public float distance = 1f;

	[Range(0.1f, 8f)]
	public float ZoomWheelSpeed = 4f;

	public float minDistance = 0.5f;

	public float maxDistance = 10f;

	public float xSpeed = 250f;

	public float ySpeed = 120f;

	public float xObjSpeed = 250f;

	public float yObjSpeed = 120f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	private float x;

	private float y;

	private float normal_angle;

	private float cur_distance;

	private float cur_xSpeed;

	private float cur_ySpeed;

	private float req_xSpeed;

	private float req_ySpeed;

	private float cur_ObjxSpeed;

	private float cur_ObjySpeed;

	private float req_ObjxSpeed;

	private float req_ObjySpeed;

	private bool DraggingObject;

	private bool lastLMBState;

	private Collider[] surfaceColliders;

	private float bounds_MaxSize = 20f;

	[HideInInspector]
	public bool disableSteering;
}
