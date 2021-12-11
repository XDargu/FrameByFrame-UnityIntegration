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
	}

	[DataContract]
	public class EventData
	{
		[DataMember]
		public UInt32 eventIdx;
		[DataMember]
		public string name;
		[DataMember]
		public string tag;
		[DataMember]
		private PropertyGroup properties;

		public EventData(UInt32 idx, string eventName, string eventTag = "")
        {
			eventIdx = idx;
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
	}

	// #TODO: This should be part of an interface
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
		public GameObject gameObject;

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
		public float elapsedTime;
		[DataMember]
		public List<EntityData> entities;

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
		RecordingOptionChanged
	}

	[DataContract]
	public class FrameMessage
	{
		[DataMember]
		public MessageType type;

		[DataMember]
		public FrameData data;
	}

	[DataContract]
	public class RecordingOptionsMessage
	{
		[DataMember]
		public MessageType type;

		[DataMember]
		public List<RecordingOption> data;
	}

	[DataContract]
	public class RecordingOptionChangedMessage
	{
		[DataMember]
		public MessageType type;

		[DataMember]
		public RecordingOption data;
	}

	public class RecorderNetworkWebSocket : INetworkWebSocket
	{
		private List<TcpClient> m_tcpClients;

		private FrameData frameData;

		private UInt32 eventIdx;

		public UInt32 GetNextEventIdx() { return eventIdx++; }

		// TODO: Helper for frame data (fast entity access)
		// private Dictionary<UInt32, EntityData> registeredEntities;

		public EntityData RecordEntity(GameObject entity)
		{
			UInt32 entityId = (UInt32)entity.GetInstanceID();
			foreach (EntityData storedEntity in frameData.entities)
			{
				if (storedEntity.id == entityId)
				{
					return storedEntity;
				}
			}

			// Add a new one
			EntityData entityData = new EntityData(entityId);
			entityData.gameObject = entity;
			frameData.entities.Add(entityData);
			if (entity.transform.parent)
            {
				entityData.parentId = (UInt32)entity.transform.parent.gameObject.GetInstanceID();
				RecordEntity(entity.transform.parent.gameObject);
            }

			return entityData;
		}

		private void RegisterEntity(GameObject entity)
        {

        }

		public override void Init(WebSocketServer server)
		{
			m_server = server;
			m_server.RegisterNetworkWebSocket(this);
			frameData = new FrameData();
			m_tcpClients = new List<TcpClient>();
			eventIdx = 0;

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
			dynamic result = JsonConvert.DeserializeObject(data, settings);
			MessageType type = result.type;

			if (type == MessageType.RecordingOptionChanged)
            {
				RecordingOptionChangedMessage message = JsonConvert.DeserializeObject< RecordingOptionChangedMessage>(data, settings);
				FbFManager.SetRecordingOption(message.data.name, message.data.enabled);
            }
		}

		public override void Shutdown()
		{
			m_server.UnRegisterNetworkWebSocket(this);
		}

		public override void Update()
		{
			if (EditorApplication.isPaused)
            {
				return;
            }
			if (!Application.isPlaying)
            {
				return;
            }
			frameData.frameId = (UInt32)Time.frameCount;
			frameData.clientId = 0;
			frameData.serverTime = (UInt32)Math.Round(Time.timeSinceLevelLoadAsDouble * 1000); // Server time in ms
			frameData.tag = "Server";
			frameData.elapsedTime = Time.deltaTime;

			// Add special properties
			foreach (EntityData entity in frameData.entities)
			{
				entity.AddSpecialProperty("name", entity.gameObject.name);
				entity.AddSpecialProperty("position", entity.gameObject.transform.position);
			}

			// Send frame data every frame
			FrameMessage message = new FrameMessage ();
			message.data = frameData;
			message.type = MessageType.FrameData;

			JsonSerializerSettings settings = new JsonSerializerSettings{
				ReferenceLoopHandling = ReferenceLoopHandling.Error
			};
			string data = JsonConvert.SerializeObject(message, settings);

			foreach (TcpClient client in m_tcpClients)
			{
				m_server.SendData(client, data);
			}

            {
				// Server test
				frameData.frameId = (UInt32)Time.frameCount;
				frameData.clientId = 1;
				frameData.serverTime = (UInt32)Math.Round((Time.timeSinceLevelLoadAsDouble - Time.deltaTime) * 1000);
				frameData.tag = "Client";
				// Send frame data every frame
				FrameMessage message2 = new FrameMessage();
				message2.data = frameData;
				message2.type = MessageType.FrameData;

				string data2 = JsonConvert.SerializeObject(message2, settings);

				foreach (TcpClient client in m_tcpClients)
				{
					m_server.SendData(client, data2);
				}
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

			/*using (MemoryStream memoryStream = new MemoryStream ())
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FrameMessage));

				serializer.WriteObject(memoryStream, message);
				memoryStream.Position = 0;


				StreamReader reader = new StreamReader(memoryStream);
				string data = reader.ReadToEnd();


				foreach (TcpClient client in m_tcpClients)
				{
					m_server.SendData(client, data);
				}
			}*/

			frameData.entities.Clear();
			eventIdx = 0;

			/*if (Input.GetKeyDown (KeyCode.G)) {
				FrameData frameData = new FrameData();
				frameData.frameId = (UInt32)Time.frameCount;
				frameData.elapsedTime = Time.deltaTime;
				frameData.entities = new List<EntityData>();

				EntityData entityData = new EntityData();
				entityData.id = 2;
				entityData.properties = new List<PropertyData>();
				entityData.events = new List<EventData>();

				PropertyData property = new PropertyData();
				property.name = "Test";
				property.type = "string";
				property.value = "hello world";

				entityData.properties.Add(property);

				frameData.entities.Add(entityData);

				FrameMessage message = new FrameMessage ();
				message.data = frameData;
				message.type = MessageType.FrameData;

				using (MemoryStream memoryStream = new MemoryStream ())
				{
					DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings ();
					settings.UseSimpleDictionaryFormat = true;

					DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FrameMessage), settings);

					serializer.WriteObject(memoryStream, message);
					memoryStream.Position = 0;

					StreamReader reader = new StreamReader(memoryStream);
					string data = reader.ReadToEnd();

					foreach (TcpClient client in m_tcpClients)
					{
						m_server.SendData(client, data);
					}
				}
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				TestData testData = new TestData();
				testData.entityId = 12;
				testData.entityName = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut l";
				foreach (TcpClient client in m_tcpClients)
				{
					m_server.SendData(client, JsonUtility.ToJson(testData));
				}
			}

			if (Input.GetKeyDown(KeyCode.Q))
			{
				TestData testData = new TestData();
				testData.entityId = 12;
				testData.entityName = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut la";
				foreach (TcpClient client in m_tcpClients)
				{
					m_server.SendData(client, JsonUtility.ToJson(testData));
				}
			}

			if (Input.GetKeyDown(KeyCode.W))
			{
				TestData testData = new TestData();
				testData.entityId = 12;
				testData.entityName = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
				foreach (TcpClient client in m_tcpClients)
				{
					m_server.SendData(client, JsonUtility.ToJson(testData));
				}
			}*/
		}
	}
}
