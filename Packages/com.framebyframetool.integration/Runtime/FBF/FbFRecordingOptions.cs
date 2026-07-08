using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FbF
{
	public static class FbFRecordingOptions
	{
		private const string EditorPrefsPrefix = "FrameByFrame.RecordingOption.";

		private static readonly object m_lock = new object();
		private static readonly Dictionary<string, FrameByFrameRecordingOptionDefinition> m_definitions = new Dictionary<string, FrameByFrameRecordingOptionDefinition>();
		private static readonly Dictionary<string, bool> m_states = new Dictionary<string, bool>();
		private static bool m_hasDiscoveredOptions;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void DiscoverBeforeSceneLoad()
		{
			DiscoverOptions();
		}

		public static void DiscoverOptions()
		{
			lock (m_lock)
			{
				if (m_hasDiscoveredOptions)
				{
					return;
				}

				m_hasDiscoveredOptions = true;
			}

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException exception)
				{
					types = exception.Types;
				}

				if (types == null)
				{
					continue;
				}

				foreach (Type type in types)
				{
					if (type == null)
					{
						continue;
					}

					object[] attributes = type.GetCustomAttributes(typeof(FrameByFrameRecordingOptionAttribute), true);
					foreach (object attribute in attributes)
					{
						FrameByFrameRecordingOptionAttribute recordingOption = attribute as FrameByFrameRecordingOptionAttribute;
						if (recordingOption != null)
						{
							Register(recordingOption.Id, recordingOption.Description);
						}
					}
				}
			}
		}

		public static void Register(string id, string description)
		{
			if (string.IsNullOrEmpty(id))
			{
				Debug.LogWarning("Frame by Frame recording options require a non-empty id.");
				return;
			}

			lock (m_lock)
			{
				FrameByFrameRecordingOptionDefinition definition;
				if (m_definitions.TryGetValue(id, out definition))
				{
					if (string.IsNullOrEmpty(definition.description) && !string.IsNullOrEmpty(description))
					{
						definition.description = description;
					}
				}
				else
				{
					m_definitions[id] = new FrameByFrameRecordingOptionDefinition(id, description);
				}

				if (!m_states.ContainsKey(id))
				{
					m_states[id] = LoadState(id);
				}
			}
		}

		public static bool IsEnabled(string id)
		{
			lock (m_lock)
			{
				bool isEnabled;
				return m_states.TryGetValue(id, out isEnabled) && isEnabled;
			}
		}

		public static void SetEnabled(string id, bool enabled)
		{
			Register(id, string.Empty);

			lock (m_lock)
			{
				m_states[id] = enabled;
			}

			SaveState(id, enabled);
		}

		public static List<FrameByFrameRecordingOptionDefinition> GetDefinitions()
		{
			DiscoverOptions();

			lock (m_lock)
			{
				List<FrameByFrameRecordingOptionDefinition> result = new List<FrameByFrameRecordingOptionDefinition>(m_definitions.Values);
				result.Sort((left, right) => string.Compare(left.id, right.id, StringComparison.OrdinalIgnoreCase));
				return result;
			}
		}

		public static Dictionary<string, bool> GetStates()
		{
			DiscoverOptions();

			lock (m_lock)
			{
				return new Dictionary<string, bool>(m_states);
			}
		}

		public static List<RecordingOption> GetRecordingOptions()
		{
			List<FrameByFrameRecordingOptionDefinition> definitions = GetDefinitions();
			List<RecordingOption> result = new List<RecordingOption>();

			foreach (FrameByFrameRecordingOptionDefinition definition in definitions)
			{
				RecordingOption option = new RecordingOption();
				option.name = definition.id;
				option.description = definition.description;
				option.enabled = IsEnabled(definition.id);
				result.Add(option);
			}

			return result;
		}

		public static void ApplyStates(IEnumerable<FrameByFrameRecordingOptionState> states)
		{
			if (states == null)
			{
				return;
			}

			foreach (FrameByFrameRecordingOptionState state in states)
			{
				if (state != null && !string.IsNullOrEmpty(state.id))
				{
					SetEnabled(state.id, state.enabled);
				}
			}
		}

		public static void Persist()
		{
			Dictionary<string, bool> states = GetStates();
			foreach (KeyValuePair<string, bool> entry in states)
			{
				SaveState(entry.Key, entry.Value);
			}
		}

		private static bool LoadState(string id)
		{
#if UNITY_EDITOR
			string key = GetEditorPrefsKey(id);
			if (EditorPrefs.HasKey(key))
			{
				return EditorPrefs.GetBool(key);
			}

			if (EditorPrefs.HasKey(id))
			{
				return EditorPrefs.GetBool(id);
			}
#endif
			return false;
		}

		private static void SaveState(string id, bool enabled)
		{
#if UNITY_EDITOR
			EditorPrefs.SetBool(GetEditorPrefsKey(id), enabled);
#endif
		}

		private static string GetEditorPrefsKey(string id)
		{
			return EditorPrefsPrefix + id;
		}
	}
}
