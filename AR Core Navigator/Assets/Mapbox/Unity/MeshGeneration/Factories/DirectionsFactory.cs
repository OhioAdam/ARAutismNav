namespace Mapbox.Unity.MeshGeneration.Factories
{
	using UnityEngine;
	using Mapbox.Directions;
	using System.Collections.Generic;
	using System.Linq;
	using Mapbox.Unity.Map;
	using Data;
	using Modifiers;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using UnityEngine.UI;

	public class DirectionsFactory : MonoBehaviour
	{

		[SerializeField]
		bool debug;

		[SerializeField]
		Text Testtext;
        int Speed = 2;
        float changeInterval = 0.5f;
        [SerializeField]
		AbstractMap _map;

		[SerializeField]
		MeshModifier[] MeshModifiers;
		public Transform[] _waypoints;

		public Vector2d[] wp;

		[SerializeField]
		Material _material;

		[SerializeField]
		float _directionsLineWidth;

		private Directions _directions;
		private int _counter;
		private bool start;
		private bool check1;
		private bool check2;
		private bool check3;
		private bool que;
		private bool respond;
		private bool creation;
		private bool created;

		GameObject _directionsGO;

		
		
		LineRenderer line;

		[HideInInspector]
		public string GameObjectname;

		private string LastGOname;

		protected virtual void Awake()
		{
			if(start != true && debug)
			{
				Testtext.text = "started";
				start = true;
			}
			
			if (_map == null)
			{
				_map = FindObjectOfType<AbstractMap>();
			}
			_directions = MapboxAccess.Instance.Directions;
			_map.OnInitialized += Query;
			_map.OnUpdated += Query;
		}

		protected virtual void OnDestroy()
		{
			_map.OnInitialized -= Query;
			_map.OnUpdated -= Query;
		}

		public void Query()
		{
			if(que != true && debug)
			{
				Testtext.text = "In Query";
				que = true;
			}
			/* 

			var count = _waypoints.Length;
			var wp = new Vector2d[count];
			
			for (int i = 0; i < count; i++)
			{
				wp[i] = _waypoints[i].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			}
			*/
			var _directionResource = new DirectionResource(wp, RoutingProfile.Walking);
			_directionResource.Steps = true;
			_directions.Query(_directionResource, HandleDirectionsResponse);
		}

		void HandleDirectionsResponse(DirectionsResponse response)
		{
			if(respond != true && debug)
			{
				Testtext.text = "in Response";
				respond = true;
			}

			if (null == response.Routes || response.Routes.Count < 1)
			{
				return;
			}

			if(check1 != true && debug)
			{
				Testtext.text = "in checkpoint 1";
				check1 = true;
			}

			var meshData = new MeshData();
			var dat = new List<Vector3>();
			foreach (var point in response.Routes[0].Geometry)
			{
				dat.Add(Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());
			}

			if(check2 != true && debug)
			{
				Testtext.text = "in checkpoint 2";
				check2 = true;
			}

			
			var feat = new VectorFeatureUnity();
			feat.Points.Add(dat);
			 
			foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active))
			{ 
				var lineMod = mod as LineMeshModifier;
				if (lineMod != null)
				{
					//lineMod.Width = _directionsLineWidth / _map.WorldRelativeScale;
				}
				
				//mod.Run(feat, meshData, _map.WorldRelativeScale);
				
			}
			

			if(check3 != true && debug)
			{
				Testtext.text = "in checkpoint 3";
				check3 = true;
			}
			
			if(!GameObject.Find(GameObjectname) && GameObjectname != "")
			{
				CreateGameObject(dat);
			}


		}

		GameObject CreateGameObject(List<Vector3> data)
		{
			if(creation != true && debug)
			{
				Testtext.text = "GameObject creation call";
				creation = true;
			}
			_directionsGO = new GameObject(GameObjectname);
			LastGOname = GameObjectname;
			if(debug)
			{
				Testtext.text = "Obj created";
			}
            _directionsGO.transform.position = new Vector3(0, .1f, 0);
			//_directionsGO.transform.SetParent(GameObject.Find("Map").transform);
			_directionsGO.tag = "Map";
            line = _directionsGO.AddComponent<LineRenderer>();
            line.material = _material;
            line.useWorldSpace = false;
			Vector3[] points = data.ToArray();
     		line.positionCount = points.Length;
     		line.SetPositions(points);
            //line.material=Resources.Load("Materials/chevron_line.mat", typeof(Material)) as Material;
            //line.material.color = new Color(0,1,0);
            //line.positionCount = data.Vertices.Count/10;
            //line.SetPositions(data.Vertices.ToArray());
			//_counter = data.Triangles.Count;
			return _directionsGO;
		}
        void Update()
        {
			if(LastGOname != GameObjectname)
			{
				Destroy(GameObject.Find(LastGOname));
			}
			Query();
        }
    }

}

