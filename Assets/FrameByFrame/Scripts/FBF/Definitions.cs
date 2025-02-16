using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace FbF
{
	[DataContract]
	public class Vector2Json
	{
		[DataMember]
		float x;
		[DataMember]
		float y;

		public Vector2Json(Vector2 value)
		{
			x = value.x;
			y = value.y;
		}
	}

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

	public struct Icon
    {
		public string icon;
		public string color;

		public Icon(string icon, string color)
		{
			this.icon = icon;
			this.color = color;
		}

		public Icon(string icon)
		{
			this.icon = icon;
			this.color = "";
		}

		public static readonly Icon None = new Icon("", "");
	}

	public interface IPropertyData
	{
		string type { get; set; }
		string name { get; set; }
		string icon { get; set; }
		string icolor { get; set; }
	}

	[Flags]
	public enum PropertyFlags
	{
		None = 0,
		Hidden = 1 << 0,
		Collapsed = 1 << 1
	}

	[DataContract]
	public class PropertyData<T> : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }
		[DataMember]
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }
		[DataMember]
		public T value;

		public PropertyData(string propType, string propName, T propValue, Icon ? icon, PropertyFlags flags = PropertyFlags.None)
		{
			type = propType;
			name = propName;
			value = propValue;
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }
		[DataMember]
		public string value;

		public PropertyComment(string value, Icon? icon)
		{
			type = "comment";
			this.value = value;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
	public class Table
	{
		[DataMember]
		public List<string> header;

		[DataMember]
		public List<List<string>> rows;

		public Table(params string[] header)
		{
			this.header = new List<string>(header);
			this.rows = new List<List<string>>();
		}

		public void AddRow(params string[] entries)
		{
			if (entries.Length == this.header.Count)
				this.rows.Add(new List<string>(entries));
			else
				Debug.LogWarning("Adding entries to a table with a different number of header elements");
		}
	}

	[DataContract]
	public class PropertyTable : IPropertyData
	{
		[DataMember]
		public string type { get; set; }

		[DataMember]
		public string name { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

		[DataMember]
		public Table value { get; set; }

		public PropertyTable(string name, params string[] header)
		{
			type = "table";
			this.name = name;
			this.value = new Table(header);
		}

		public void AddRow(params string[] entries)
		{
			this.value.AddRow(entries);
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
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

		[DataMember]
		public EntityRef value;

		public PropertyEntity(string name, GameObject entity, Icon? icon)
		{
			type = "eref";
			this.value = new EntityRef(entity);
			this.name = name;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

		[DataMember]
		public Vector3Json position;
		[DataMember]
		public float radius;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertySphere(string name, Vector3 position, float radius, Color color, string layer, Icon? icon, PropertyFlags flags)
		{
			this.type = "sphere";
			this.name = name;
			this.position = new Vector3Json(position);
			this.radius = radius;
			this.color = new ColorJson(color);
			this.layer = layer;
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

		[DataMember]
		public Vector3Json position;
		[DataMember]
		public Vector3Json size;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyAABB(string name, Vector3 position, Vector3 size, Color color, string layer, Icon? icon, PropertyFlags flags)
		{
			this.type = "aabb";
			this.name = name;
			this.position = new Vector3Json(position);
			this.size = new Vector3Json(size);
			this.color = new ColorJson(color);
			this.layer = layer;
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

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

		public PropertyOOBB(string name, Vector3 position, Vector3 size, Vector3 up, Vector3 forward, Color color, string layer, Icon? icon, PropertyFlags flags)
		{
			this.type = "oobb";
			this.name = name;
			this.position = new Vector3Json(position);
			this.size = new Vector3Json(size);
			this.up = new Vector3Json(up);
			this.forward = new Vector3Json(forward);
			this.color = new ColorJson(color);
			this.layer = layer;
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
		}
	}

	public class PropertyCapsule : IPropertyData
	{
		[DataMember]
		public string type { get; set; }
		[DataMember]
		public string name { get; set; }
		[DataMember]
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

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

		public PropertyCapsule(string name, Vector3 position, Vector3 direction, float radius, float height, Color color, string layer, Icon? icon, PropertyFlags flags)
		{
			this.type = "capsule";
			this.name = name;
			this.position = new Vector3Json(position);
			this.direction = new Vector3Json(direction);
			this.radius = radius;
			this.height = height;
			this.color = new ColorJson(color);
			this.layer = layer;
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

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
		[DataMember]
		public string texture;

		public PropertyPlane(string name, Vector3 position, Vector3 normal, Vector3 up, float width, float length, Color color, string texture, string layer, Icon? icon, PropertyFlags flags)
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
			this.flags = flags;
			this.texture = texture;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

		[DataMember]
		public Vector3Json origin;
		[DataMember]
		public Vector3Json destination;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyLine(string name, Vector3 origin, Vector3 destination, Color color, string layer, Icon? icon, PropertyFlags flags)
		{
			this.type = "line";
			this.name = name;
			this.origin = new Vector3Json(origin);
			this.destination = new Vector3Json(destination);
			this.color = new ColorJson(color);
			this.layer = layer;
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

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

		public PropertyMesh(string name, float[] vertices, int[] indices, bool wireframe, Color color, string layer, Icon? icon, PropertyFlags flags)
		{
			this.type = "mesh";
			this.name = name;
			this.vertices = vertices;
			this.indices = indices;
			this.wireframe = wireframe;
			this.color = new ColorJson(color);
			this.layer = layer;
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

		[DataMember]
		public Vector3Json[] points;
		[DataMember]
		public string layer;
		[DataMember]
		public ColorJson color;

		public PropertyPath(string name, Vector3[] points, Color color, string layer, Icon? icon, PropertyFlags flags)
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
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }

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

		public PropertyTriangle(string name, Vector3 p1, Vector3 p2, Vector3 p3, Color color, string layer, Icon? icon, PropertyFlags flags)
		{
			this.type = "triangle";
			this.name = name;
			this.p1 = new Vector3Json(p1);
			this.p2 = new Vector3Json(p2);
			this.p3 = new Vector3Json(p3);
			this.layer = layer;
			this.color = new ColorJson(color);
			this.flags = flags;
			if (icon.HasValue)
			{
				this.icon = icon.Value.icon;
				this.icolor = icon.Value.color;
			}
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
		public PropertyFlags flags { get; set; }
		[DataMember]
		public string icon { get; set; }
		[DataMember]
		public string icolor { get; set; }
		[DataMember]
		internal List<IPropertyData> value;

		public PropertyGroup(string name)
		{
			this.name = name;
			this.type = "group";
			this.value = new List<IPropertyData>();
		}
		public void AddProperty<T>(string name, T value, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			if (typeof(T) == typeof(bool))
			{
				this.value.Add(new PropertyData<bool>("boolean", name, (bool)(object)value, icon, flags));
			}
			else if (typeof(T) == typeof(string))
			{
				this.value.Add(new PropertyData<string>("string", name, (string)(object)value, icon, flags));
			}
			else if (typeof(T) == typeof(int))
			{
				this.value.Add(new PropertyData<int>("number", name, (int)(object)value, icon, flags));
			}
			else if (typeof(T) == typeof(float))
			{
				this.value.Add(new PropertyData<float>("number", name, (float)(object)value, icon, flags));
			}
			else if (typeof(T) == typeof(Vector2))
			{
				this.value.Add(new PropertyData<Vector2Json>("vec2", name, new Vector2Json((Vector2)(object)value), icon, flags));
			}
			else if (typeof(T) == typeof(Vector3))
			{
				this.value.Add(new PropertyData<Vector3Json>("vec3", name, new Vector3Json((Vector3)(object)value), icon, flags));
			}
			else
			{
				Debug.LogError("Not implemented");
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

		public PropertyComment AddComment(string text, Icon? icon = null)
		{
			PropertyComment comment = new PropertyComment(text, icon);
			this.value.Add(comment);
			return comment;
		}

		public PropertyEntity AddEntityRef(string name, GameObject entity, Icon? icon = null)
		{
			PropertyEntity entityRef = new PropertyEntity(name, entity, icon);
			this.value.Add(entityRef);
			return entityRef;
		}

		public PropertyTable AddTable(string name, params string[] header)
		{
			PropertyTable table = new PropertyTable(name, header);
			this.value.Add(table);
			return table;
		}

		public PropertySphere AddSphere(string name, Vector3 position, float radius, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertySphere sphere = new PropertySphere(name, position, radius, color, layer, icon, flags);
			this.value.Add(sphere);
			return sphere;
		}

		public PropertyAABB AddAABB(string name, Vector3 position, Vector3 size, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyAABB aabb = new PropertyAABB(name, position, size, color, layer, icon, flags);
			this.value.Add(aabb);
			return aabb;
		}

		public PropertyOOBB AddOOBB(string name, Vector3 position, Vector3 size, Vector3 up, Vector3 forward, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyOOBB oobb = new PropertyOOBB(name, position, size, up, forward, color, layer, icon, flags);
			this.value.Add(oobb);
			return oobb;
		}

		public PropertyCapsule AddCapsule(string name, Vector3 position, Vector3 direction, float radius, float height, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyCapsule capsule = new PropertyCapsule(name, position, direction, radius, height, color, layer, icon, flags);
			this.value.Add(capsule);
			return capsule;
		}

		public PropertyPlane AddPlane(string name, Vector3 position, Vector3 normal, Vector3 up, float width, float length, Color color, string texture, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyPlane plane = new PropertyPlane(name, position, normal, up, width, length, color, texture, layer, icon, flags);
			this.value.Add(plane);
			return plane;
		}

		public PropertyLine AddLine(string name, Vector3 origin, Vector3 destination, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyLine line = new PropertyLine(name, origin, destination, color, layer, icon, flags);
			this.value.Add(line);
			return line;
		}

		public PropertyMesh AddMesh(string name, float[] vertices, int[] indices, bool wireframe, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyMesh mesh = new PropertyMesh(name, vertices, indices, wireframe, color, layer, icon, flags);
			this.value.Add(mesh);
			return mesh;
		}

		public PropertyPath AddPath(string name, Vector3[] points, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyPath path = new PropertyPath(name, points, color, layer, icon, flags);
			this.value.Add(path);
			return path;
		}

		public PropertyTriangle AddTriangle(string name, Vector3 p1, Vector3 p2, Vector3 p3, Color color, string layer, Icon? icon = null, PropertyFlags flags = PropertyFlags.None)
		{
			PropertyTriangle triangle = new PropertyTriangle(name, p1, p2, p3, color, layer, icon, flags);
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
		public PropertyGroup properties;

		public EventData(UInt32 index, string eventName, string eventTag = "")
		{
			idx = index;
			name = eventName;
			tag = eventTag;
			properties = new PropertyGroup("properties");
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
			EventData addedEvent = new EventData(FbFManager.recorder.GetNextEventIdx(), name, tag);
			events.Add(addedEvent);
			return addedEvent;
		}

		public PropertyGroup AddPropertyGroup(string name)
		{
			return properties[0].AddPropertyGroup(name);
		}

		public void AddSpecialProperty<T>(string name, T value)
		{
			properties[1].AddProperty(name, value);
		}
	}

	[DataContract]
	public class Resource
    {
		[DataMember]
		public string path;
		[DataMember]
		public string textData;
		[DataMember]
		public string type;

		public Resource(string path, string type, string content)
        {
			this.path = path;
			this.type = type;
			this.textData = content;
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
		[DataMember]
		public List<Resource> resources;

		public FrameData()
		{
			entities = new List<EntityData>();
			resources = new List<Resource>();
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
}