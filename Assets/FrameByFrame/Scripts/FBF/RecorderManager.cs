using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace FbF
{
	[DataContract]
	public class Vector3Json
	{
		[DataMember]
		float x;
		[DataMember]
		float y;
		[DataMember]
		float z;

		public Vector3Json(Vector3 value)
        {
			x = value.x;
			y = value.y;
			z = value.z;
		}
	}

	[DataContract]
	public class ColorJson
	{
		[DataMember]
		float r;
		[DataMember]
		float g;
		[DataMember]
		float b;
		[DataMember]
		float a;

		public ColorJson(Color value)
		{
			r = value.r;
			g = value.g;
			b = value.b;
			a = value.a;
		}
	}
	public interface IPropertyData
	{
		string type { get; set; }
		string name { get; set; }
	}

	[DataContract]
	public class PropertyData<T> : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }
		[DataMember]
		public T value;

		public PropertyData(string propType, string propName, T propValue)
		{
			type = propType;
			name = propName;
			value = propValue;
		}
	}

	[DataContract]
	public class PropertyComment : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }
		[DataMember]
		public string value;

		public PropertyComment(string value)
		{
			type = "comment";
			this.value = value;
		}
	}

	[DataContract]
	public class EntityRef
	{
		[DataMember]
		public UInt32 id;
		[DataMember]
		public string name { get; set; }

		public EntityRef(GameObject entity)
		{
			this.id = RecorderNetworkWebSocket.GetEntityID(entity);
			this.name = entity.name;
		}
	}

	[DataContract]
	public class PropertyEntity : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }
		[DataMember]
		public EntityRef value;

		public PropertyEntity(string name, GameObject entity)
		{
			type = "eref";
			this.value = new EntityRef(entity);
			this.name = name;
		}
	}

	[DataContract]
	public class PropertySphere : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json position;
		[DataMember]
		public float radius;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertySphere(string name, Vector3 position, float radius, Color color, string layer)
		{
			this.type = "sphere";
			this.name = name;
			this.position = new Vector3Json(position);
			this.radius = radius;
			this.color = new ColorJson(color);
			this.layer = layer;
		}
	}

	[DataContract]
	public class PropertyAABB : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json position;
		[DataMember]
		public Vector3Json size;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyAABB(string name, Vector3 position, Vector3 size, Color color, string layer)
		{
			this.type = "aabb";
			this.name = name;
			this.position = new Vector3Json(position);
			this.size = new Vector3Json(size);
			this.color = new ColorJson(color);
			this.layer = layer;
		}
	}

	[DataContract]
	public class PropertyOOBB : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json position;
		[DataMember]
		public Vector3Json size;

		[DataMember]
		public Vector3Json up;
		[DataMember]
		public Vector3Json forward;

		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyOOBB(string name, Vector3 position, Vector3 size, Vector3 up, Vector3 forward, Color color, string layer)
		{
			this.type = "oobb";
			this.name = name;
			this.position = new Vector3Json(position);
			this.size = new Vector3Json(size);
			this.up = new Vector3Json(up);
			this.forward = new Vector3Json(forward);
			this.color = new ColorJson(color);
			this.layer = layer;
		}
	}

	public class PropertyCapsule : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json position;
		[DataMember]
		public Vector3Json direction;

		[DataMember]
		public float radius;
		[DataMember]
		public float height;

		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyCapsule(string name, Vector3 position, Vector3 direction, float radius, float height, Color color, string layer)
		{
			this.type = "capsule";
			this.name = name;
			this.position = new Vector3Json(position);
			this.direction = new Vector3Json(direction);
			this.radius = radius;
			this.height = height;
			this.color = new ColorJson(color);
			this.layer = layer;
		}
	}

	[DataContract]
	public class PropertyPlane : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json position;
		[DataMember]
		public Vector3Json normal;
		[DataMember]
		public Vector3Json up;
		[DataMember]
		public float width;
		[DataMember]
		public float length;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyPlane(string name, Vector3 position, Vector3 normal, Vector3 up, float width, float length, Color color, string layer)
		{
			this.type = "plane";
			this.name = name;
			this.position = new Vector3Json(position);
			this.normal = new Vector3Json(normal);
			this.up = new Vector3Json(up);
			this.width = width;
			this.length = length;
			this.color = new ColorJson(color);
			this.layer = layer;
		}
	}

	[DataContract]
	public class PropertyLine : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json origin;
		[DataMember]
		public Vector3Json destination;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyLine(string name, Vector3 origin, Vector3 destination, Color color, string layer)
		{
			this.type = "line";
			this.name = name;
			this.origin = new Vector3Json(origin);
			this.destination = new Vector3Json(destination);
			this.color = new ColorJson(color);
			this.layer = layer;
		}
	}

	[DataContract]
	public class PropertyMesh : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public bool wireframe { get; set; }

		[DataMember]
		public float[] vertices;
		[DataMember]
		public int[] indices;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyMesh(string name, float[] vertices, int[] indices, bool wireframe, Color color, string layer)
		{
			this.type = "mesh";
			this.name = name;
			this.vertices = vertices;
			this.indices = indices;
			this.wireframe = wireframe;
			this.color = new ColorJson(color);
			this.layer = layer;
		}
	}

	[DataContract]
	public class PropertyPath : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json[] points;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyPath(string name, Vector3[] points, Color color, string layer)
		{
			this.type = "path";
			this.name = name;
			this.points = new Vector3Json[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				this.points[i] = new Vector3Json(points[i]);
			}
			this.layer = layer;
			this.color = new ColorJson(color);
		}
	}

	[DataContract]
	public class PropertyTriangle : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }

		[DataMember]
		public Vector3Json p1;
		[DataMember]
		public Vector3Json p2;
		[DataMember]
		public Vector3Json p3;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyTriangle(string name, Vector3 p1, Vector3 p2, Vector3 p3, Color color, string layer)
		{
			this.type = "triangle";
			this.name = name;
			this.p1 = new Vector3Json(p1);
			this.p2 = new Vector3Json(p2);
			this.p3 = new Vector3Json(p3);
			this.layer = layer;
			this.color = new ColorJson(color);
		}
	}

	[DataContract]
	[KnownType(typeof(PropertyData<string>))]
	[KnownType(typeof(PropertyData<float>))]
	[KnownType(typeof(PropertyData<Vector3Json>))]
	[KnownType(typeof(PropertySphere))]
	[KnownType(typeof(PropertyAABB))]
	[KnownType(typeof(PropertyOOBB))]
	[KnownType(typeof(PropertyLine))]
	[KnownType(typeof(PropertyPlane))]
	[KnownType(typeof(PropertyGroup))]
	public class PropertyGroup : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }
		[DataMember]
		internal List<IPropertyData> value;

		public PropertyGroup(string name)
		{
			this.name = name;
			this.type = "group";
			this.value = new List<IPropertyData>();
		}
		public void AddProperty<T>(string name, T value)
		{
			if (typeof(T) == typeof(bool))
			{
				this.value.Add(new PropertyData<bool>("boolean", name, (bool)(object)value));
			}
			else if (typeof(T) == typeof(string))
			{
				this.value.Add (new PropertyData<string> ("string", name, (string)(object)value));
			}
			else if (typeof(T) == typeof(int))
			{
				this.value.Add(new PropertyData<int>("number", name, (int)(object)value));
			}
			else if (typeof(T) == typeof(float))
			{
				this.value.Add (new PropertyData<float> ("number", name, (float)(object)value));
			}
			else if (typeof(T) == typeof(Vector3))
			{
				this.value.Add (new PropertyData<Vector3Json> ("vec3", name, new Vector3Json((Vector3)(object)value)));
			}
			else
			{
				Debug.LogError ("Not implemented");
			}
		}

		public PropertyGroup AddPropertyGroup(string name)
		{
			IPropertyData existingGroup = this.value.FindLast((property) => { return property.name == name && property.type == "group"; });
			if (existingGroup != null)
			{
				return ((PropertyGroup)existingGroup);
			}
			else
			{
				PropertyGroup group = new PropertyGroup(name);
				this.value.Add(group);
				return group;
			}
		}

		public PropertyComment AddComment(string text)
        {
			PropertyComment comment = new PropertyComment(text);
			this.value.Add(comment);
			return comment;
		}

		public PropertyEntity AddEntityRef(string name, GameObject entity)
		{
			PropertyEntity entityRef = new PropertyEntity(name, entity);
			this.value.Add(entityRef);
			return entityRef;
		}

		public PropertySphere AddSphere(string name, Vector3 position, float radius, Color color, string layer)
		{
			PropertySphere sphere = new PropertySphere(name, position, radius, color, layer);
			this.value.Add(sphere);
			return sphere;
		}

		public PropertyAABB AddAABB(string name, Vector3 position, Vector3 size, Color color, string layer)
		{
			PropertyAABB aabb = new PropertyAABB(name, position, size, color, layer);
			this.value.Add(aabb);
			return aabb;
		}

		public PropertyOOBB AddOOBB(string name, Vector3 position, Vector3 size, Vector3 up, Vector3 forward, Color color, string layer)
		{
			PropertyOOBB oobb = new PropertyOOBB(name, position, size, up, forward, color, layer);
			this.value.Add(oobb);
			return oobb;
		}

		public PropertyCapsule AddCapsule(string name, Vector3 position, Vector3 direction, float radius, float height, Color color, string layer)
		{
			PropertyCapsule capsule = new PropertyCapsule(name, position, direction, radius, height, color, layer);
			this.value.Add(capsule);
			return capsule;
		}

		public PropertyPlane AddPlane(string name, Vector3 position, Vector3 normal, Vector3 up, float width, float length, Color color, string layer)
		{
			PropertyPlane plane = new PropertyPlane(name, position, normal, up, width, length, color, layer);
			this.value.Add(plane);
			return plane;
		}

		public PropertyLine AddLine(string name, Vector3 origin, Vector3 destination, Color color, string layer)
		{
			PropertyLine line = new PropertyLine(name, origin, destination, color, layer);
			this.value.Add(line);
			return line;
		}

		public PropertyMesh AddMesh(string name, float[] vertices, int[] indices, bool wireframe, Color color, string layer)
		{
			PropertyMesh mesh = new PropertyMesh(name, vertices, indices, wireframe, color, layer);
			this.value.Add(mesh);
			return mesh;
		}

		public PropertyPath AddPath(string name, Vector3[] points, Color color, string layer)
		{
			PropertyPath path = new PropertyPath(name, points, color, layer);
			this.value.Add(path);
			return path;
		}

		public PropertyTriangle AddTriangle(string name, Vector3 p1, Vector3 p2, Vector3 p3, Color color, string layer)
		{
			PropertyTriangle triangle = new PropertyTriangle(name, p1, p2, p3, color, layer);
			this.value.Add(triangle);
			return triangle;
		}
	}

	[DataContract]
	public class EventData
	{
		[DataMember]
		public UInt32 idx;
		[DataMember]
		public string name;
		[DataMember]
		public string tag;
		[DataMember]
		private PropertyGroup properties;

		public EventData(UInt32 index, string eventName, string eventTag = "")
        {
			idx = index;
			name = eventName;
			tag = eventTag;
			properties = new PropertyGroup("properties");
		}

		public void AddProperty<T>(string name, T value)
		{
			properties.AddProperty(name, value);
		}

		public PropertyGroup AddPropertyGroup(string name)
		{
			return properties.AddPropertyGroup(name);
		}

		public PropertyComment AddComment(string text)
		{
			return properties.AddComment(text);
		}

		public PropertyEntity AddEntityRef(string name, GameObject entity)
		{
			return properties.AddEntityRef(name, entity);
		}

		public PropertySphere AddSphere(string name, Vector3 position, float radius, Color color, string layer)
		{
			return properties.AddSphere(name, position, radius, color, layer);
		}

		public PropertyAABB AddAABB(string name, Vector3 position, Vector3 size, Color color, string layer)
		{
			return properties.AddAABB(name, position, size, color, layer);
		}

		public PropertyOOBB AddOOBB(string name, Vector3 position, Vector3 size, Vector3 up, Vector3 forward, Color color, string layer)
		{
			return properties.AddOOBB(name, position, size, up, forward, color, layer);
		}

		public PropertyPlane AddPlane(string name, Vector3 position, Vector3 normal, Vector3 up, float width, float length, Color color, string layer)
		{
			return properties.AddPlane(name, position, normal, up, width, length, color, layer);
		}

		public PropertyLine AddLine(string name, Vector3 origin, Vector3 destination, Color color, string layer)
		{
			return properties.AddLine(name, origin, destination, color, layer);
		}

		public PropertyMesh AddMesh(string name, float[] vertices, int[] indices, bool wireframe, Color color, string layer)
        {
			return properties.AddMesh(name, vertices, indices, wireframe, color, layer);
		}

		public PropertyPath AddPath(string name, Vector3[] points, Color color, string layer)
        {
			return properties.AddPath(name, points, color, layer);
        }

		public PropertyTriangle AddTriangle(string name, Vector3 p1, Vector3 p2, Vector3 p3, Color color, string layer)
        {
			return properties.AddTriangle(name, p1, p2, p3, color, layer);
		}
	}

	[DataContract]
	public class EntityData
	{
		public EntityData(UInt32 entityId)
		{
			id = entityId;
			parentId = 0;
			properties = new List<PropertyGroup>();
			properties.Add(new PropertyGroup("properties"));
			properties.Add(new PropertyGroup("special"));
			events = new List<EventData>();
		}

		[DataMember]
		public UInt32 id { get; set; }
		[DataMember]
		public UInt32 parentId { get; set; }
		[DataMember]
		private List<PropertyGroup> properties;
		[DataMember]
		private List<EventData> events;

		// Custom implementation
		public string name;
		public Vector3 position;
		public Vector3 up;
		public Vector3 forward;

		public EventData AddEvent(string name, string tag)
        {
			FbF.EventData addedEvent = new EventData(FbFManager.recorder.GetNextEventIdx(), name, tag);
			events.Add(addedEvent);
			return addedEvent;
		}

		public void AddProperty<T>(string name, T value)
		{
			properties[0].AddProperty(name, value);
		}

		public PropertyGroup AddPropertyGroup(string name)
		{
			return properties[0].AddPropertyGroup(name);
		}

		public PropertyComment AddComment(string text)
		{
			return properties[0].AddComment(text);
		}

		public PropertyEntity AddEntityRef(string name, GameObject entity)
		{
			return properties[0].AddEntityRef(name, entity);
		}

		public PropertySphere AddSphere(string name, Vector3 position, float radius, Color color, string layer)
		{
			return properties[0].AddSphere(name, position, radius, color, layer);
		}

		public PropertyAABB AddAABB(string name, Vector3 position, Vector3 size, Color color, string layer)
		{
			return properties[0].AddAABB(name, position, size, color, layer);
		}

		public PropertyOOBB AddOOBB(string name, Vector3 position, Vector3 size, Vector3 up, Vector3 forward, Color color, string layer)
		{
			return properties[0].AddOOBB(name, position, size, up, forward, color, layer);
		}

		public PropertyPlane AddPlane(string name, Vector3 position, Vector3 normal, Vector3 up, float width, float length, Color color, string layer)
		{
			return properties[0].AddPlane(name, position, normal, up, width, length, color, layer);
		}

		public PropertyLine AddLine(string name, Vector3 origin, Vector3 destination, Color color, string layer)
		{
			return properties[0].AddLine(name, origin, destination, color, layer);
		}

		public PropertyMesh AddMesh(string name, float[] vertices, int[] indices, bool wireframe, Color color, string layer)
		{
			return properties[0].AddMesh(name, vertices, indices, wireframe, color, layer);
		}

		public PropertyPath AddPath(string name, Vector3[] points, Color color, string layer)
		{
			return properties[0].AddPath(name, points, color, layer);
		}

		public PropertyTriangle AddTriangle(string name, Vector3 p1, Vector3 p2, Vector3 p3, Color color, string layer)
		{
			return properties[0].AddTriangle(name, p1, p2, p3, color, layer);
		}

		public void AddSpecialProperty<T>(string name, T value)
		{
			properties[1].AddProperty(name, value);
		}
	}

	[DataContract]
	public class FrameData
	{
		[DataMember]
		public UInt32 frameId;
		[DataMember]
		public UInt32 clientId;
		[DataMember]
		public UInt32 serverTime;
		[DataMember]
		public string tag;
		[DataMember]
		public string scene;
		[DataMember]
		public float elapsedTime;
		[DataMember]
		public List<EntityData> entities;
		[DataMember]
		public CoordinateSystem coordSystem = CoordinateSystem.LeftHand;

		public FrameData()
		{
			entities = new List<EntityData> ();
		}
	}

	public class RecordingOption
    {
		[DataMember]
		public string name;
		[DataMember]
		public bool enabled;
    }

	[DataContract]
	public enum MessageType
	{
		FrameData = 0,
		RecordingOptions,
		RecordingOptionChanged,
		SyncOptionsChanged,
		SyncVisibleShapesData,
		SyncCameraData
	}

	[DataContract]
	public class FbFMessage
    {
		[DataMember]
		public MessageType type;
	}

	[DataContract]
	public class FrameMessage : FbFMessage
	{
		[DataMember]
		public FrameData data;
	}

	[DataContract]
	public class RecordingOptionsMessage : FbFMessage
	{
		[DataMember]
		public List<RecordingOption> data;
	}

	[DataContract]
	public class RecordingOptionChangedMessage : FbFMessage
	{
		[DataMember]
		public RecordingOption data;
	}

	[DataContract]
	public class SyncCameraDataMessage : FbFMessage
	{
		[DataMember]
		public Vector3 position;

		[DataMember]
		public Vector3 up;

		[DataMember]
		public Vector3 forward;
	}

	[DataContract]
	public class RemoteEntityData
	{
		[DataMember]
		public UInt32 id;

		[DataMember]
		public string name;

		[DataMember]
		public Vector3 position;

		[DataMember]
		public IPropertyData[] shapes;
	}

	[DataContract]
	public class SyncVisibleShapesDataMessage : FbFMessage
	{
		[DataMember]
		public RemoteEntityData[] entities;

		[DataMember]
		public CoordinateSystem coordSystem;
	}

	public enum RecordingMode
    {
		NetworkConnection,
		RawData
    }

	public enum CoordinateSystem
    {
		RightHand = 0,
		LeftHand
    }

	public class RecorderNetworkWebSocket : INetworkWebSocket
	{
		private List<TcpClient> m_tcpClients;

		private FrameData frameData;

		private UInt32 eventIdx;

		private RecordingMode recordingMode;

		private bool shouldCloseRawDataFile;
		private string rawRecordingPath;

		private SyncCameraDataMessage lastCameraMessage = null;

		public static UInt32 GetEntityID(GameObject entity)
        {
			if (entity.GetInstanceID() > 0)
				return ((UInt32)entity.GetInstanceID()) + Int32.MaxValue;
			else
				return (UInt32)(entity.GetInstanceID() + Int32.MaxValue);
		}

		public UInt32 GetNextEventIdx() { return eventIdx++; }

		public RecordingMode GetRecordingMode() { return recordingMode; }

		public void StartRawRecording(string path)
		{
			recordingMode = RecordingMode.RawData;
			rawRecordingPath = path + ".fbf";
			FileUtil.DeleteFileOrDirectory(rawRecordingPath);
		}

		public void StopRawRecording()
        {
			recordingMode = RecordingMode.NetworkConnection;
			CloseRawDataFile();
		}

		// TODO: Helper for frame data (fast entity access)
		// private Dictionary<UInt32, EntityData> registeredEntities;

		public EntityData RecordEntity(GameObject entity)
		{
			UInt32 entityId = GetEntityID(entity);
			foreach (EntityData storedEntity in frameData.entities)
			{
				if (storedEntity.id == entityId)
				{
					return storedEntity;
				}
			}

			// Add a new one
			EntityData entityData = new EntityData(entityId);
			entityData.position = entity.transform.position;
			entityData.up = entity.transform.up;
			entityData.forward = entity.transform.forward;
			entityData.name = entity.name;

			frameData.entities.Add(entityData);
			if (entity.transform.parent)
            {
				entityData.parentId = GetEntityID(entity.transform.parent.gameObject);
				RecordEntity(entity.transform.parent.gameObject);
            }

			return entityData;
		}

		public override void Init(WebSocketServer server)
		{
			m_server = server;
			m_server.RegisterNetworkWebSocket(this);
			frameData = new FrameData();
			m_tcpClients = new List<TcpClient>();
			eventIdx = 0;
			recordingMode = RecordingMode.NetworkConnection;
			shouldCloseRawDataFile = false;

			// Uncomment to test raw data recording
			// StartRawRecording(Application.persistentDataPath + "/rawRecording");

			FbFManager.print("Recorder Manager Initialized");
		}

		public override void OnClientConnected(TcpClient client)
		{
			bool isFirstClient = m_tcpClients.Count == 0;
			m_tcpClients.Add(client);

			if (isFirstClient)
			{
				// Initialize data
			}

			Debug.Log("Client Connected: " + client.ToString());
		}

		public override void OnClientDisconnected(TcpClient client)
		{
			m_tcpClients.Remove(client);
			bool isLastClient = m_tcpClients.Count == 0;

			if (isLastClient)
			{
				// Initialize data
			}

			Debug.Log("Client Disconnected: " + client.ToString());
		}

		public override void OnMessage(string data)
		{
			Debug.Log("OnMessage");
			Debug.Log(data);

			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};
			FbFMessage result = JsonConvert.DeserializeObject<FbFMessage>(data, settings);
			MessageType type = result.type;

			if (type == MessageType.RecordingOptionChanged)
            {
				RecordingOptionChangedMessage message = JsonConvert.DeserializeObject<RecordingOptionChangedMessage>(data, settings);
				FbFManager.SetRecordingOption(message.data.name, message.data.enabled);
            }
			if (type == MessageType.SyncCameraData)
			{
				SyncCameraDataMessage message = JsonConvert.DeserializeObject<SyncCameraDataMessage>(data, settings);
				lastCameraMessage = message;
			}
			if (type == MessageType.SyncVisibleShapesData)
            {
				SyncVisibleShapesDataMessage message = JsonConvert.DeserializeObject<SyncVisibleShapesDataMessage>(data, settings);
				Debug.Log(message);
			}
		}

		public override void Shutdown()
		{
			if (shouldCloseRawDataFile)
			{
				CloseRawDataFile();
			}
			m_server.UnRegisterNetworkWebSocket(this);
		}

		public override void Update()
		{
			if (lastCameraMessage != null)
			{
				SceneView sceneCam = SceneView.lastActiveSceneView;
				sceneCam.camera.transform.position = lastCameraMessage.position;
				sceneCam.pivot = lastCameraMessage.position;
				sceneCam.LookAt(lastCameraMessage.position + lastCameraMessage.forward, Quaternion.LookRotation(lastCameraMessage.forward, lastCameraMessage.up));
				sceneCam.Repaint();

				lastCameraMessage = null;
			}

			if (EditorApplication.isPaused)
            {
				return;
            }
			if (!Application.isPlaying)
            {
				if (shouldCloseRawDataFile)
                {
					CloseRawDataFile();
                }
				return;
            }

			frameData.frameId = (UInt32)Time.frameCount;
			frameData.scene = SceneManager.GetActiveScene().path;
			frameData.clientId = 0; // TODO: Needs to come from the initial config
			frameData.serverTime = (UInt32)Math.Round(Time.timeSinceLevelLoadAsDouble * 1000); // Server time in ms, in a real networked application, it needs to come from the networking system
			frameData.tag = "Server"; // TODO: Needs to come from the initial config
			frameData.elapsedTime = Time.deltaTime;

			// Add special properties
			foreach (EntityData entity in frameData.entities)
			{
				// Properties need to be in this order
				entity.AddSpecialProperty("name", entity.name);
				entity.AddSpecialProperty("position", entity.position);
				entity.AddSpecialProperty("up", entity.up);
				entity.AddSpecialProperty("forward", entity.forward);
			}

			// Send frame data every frame
			switch (recordingMode)
            {
				case RecordingMode.NetworkConnection:
					SendFrameDataToClients();
					break;
				case RecordingMode.RawData:
					LogFrameDataToFile();
					break;
            }

			frameData.entities.Clear();
			eventIdx = 0;
		}

		private void CloseRawDataFile()
        {
			shouldCloseRawDataFile = false;

			string path = rawRecordingPath + ".temp";
			string pathFinalFile = rawRecordingPath;
			StreamWriter writer = new StreamWriter(path, true);
			writer.WriteLine("]}");
			writer.Close();

			// Remove .temp extension
			File.Move(path, pathFinalFile);
		}

		private void LogFrameDataToFile()
        {
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};
			string data = JsonConvert.SerializeObject(frameData, settings);

			string path = rawRecordingPath + ".temp";

			bool doesFileExist = System.IO.File.Exists(path);

			StreamWriter writer = new StreamWriter(path, true);
			if (!doesFileExist)
            {
				writer.WriteLine("{");
				writer.WriteLine("\"type\": 1,");
				writer.WriteLine("\"version\": 1,");
				writer.WriteLine("\"rawFrames\": [");
				writer.WriteLine(data);
			}
			else
            {
				writer.WriteLine(",");
				writer.WriteLine(data);
			}
			writer.Close();
			shouldCloseRawDataFile = true;
		}

		private void SendFrameDataToClients()
        {
			FrameMessage message = new FrameMessage();
			message.data = frameData;
			message.type = MessageType.FrameData;

			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};
			string data = JsonConvert.SerializeObject(message, settings);

			foreach (TcpClient client in m_tcpClients)
			{
				m_server.SendData(client, data);
			}

			// Send recording options as well. This can probably be done every couple of frames
			RecordingOptionsMessage optionsMessage = new RecordingOptionsMessage();
			optionsMessage.data = new List<RecordingOption>();
			optionsMessage.type = MessageType.RecordingOptions;
			FbFManager.FillRecordingOptions(optionsMessage.data);

			string recordingOptionsData = JsonConvert.SerializeObject(optionsMessage, settings);

			foreach (TcpClient client in m_tcpClients)
			{
				m_server.SendData(client, recordingOptionsData);
			}
		}
	}
}
