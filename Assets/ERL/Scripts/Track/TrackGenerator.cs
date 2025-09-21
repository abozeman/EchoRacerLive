using UnityEngine;
using Fusion;
using RestClient.Scripts.Clients;

public class TrackGenerator : NetworkBehaviour, ITrackAPI
{
    public RestClientTrackAPI RestClientTrackGenerator;
    public GameObject TrackExterior;
    public GameObject TrackInterior;
    public GameObject TrackStartLine;

    [Networked, Capacity(20)]
    public string TrackId { get; set; }
    public bool trackIsRendered { get; set; } = false;
    private LineRenderer m_exteriorLineRenderer;
    private LineRenderer m_interiorLineRenderer;
    private LineRenderer m_startLineRenderer;
    private TrackDefinition m_trackDefinition;


    [Networked]
    [Capacity(60)]
    public NetworkArray<Vector2> ExteriorCoordinates { get; }
    [Networked] public int ExteriorCoordinatesLength { get; set; }

    [Networked]
    [Capacity(60)]
    public NetworkArray<Vector2> InteriorCoordinates { get; }
    [Networked] public int InteriorCoordinatesLength { get; set; }
    [Networked]
    [Capacity(4)]
    public NetworkArray<Vector2> StartlineCoordinates { get; }
    [Networked] public int StartlineCoordinatesLength { get; set; }

    public float renderDelay { get; private set; } = .1f;

    private ChangeDetector _changes;

    public void OnTrackDefinitionUpdate(TrackDefinition m_trackDefinition)
    {
        if (m_trackDefinition == null) return;

        if (m_trackDefinition.geometry.coordinates[0].Length > 0)
        {
            ExteriorCoordinatesLength = m_trackDefinition.geometry.coordinates[0].Length;
            for (var i = 0; i < ExteriorCoordinatesLength; i++)
            {
                ExteriorCoordinates.Set(i, new Vector2(m_trackDefinition.geometry.coordinates[0][i][0], m_trackDefinition.geometry.coordinates[0][i][1]));
            }
        }

        if (m_trackDefinition.geometry.coordinates[1].Length > 0)
        {
            InteriorCoordinatesLength = m_trackDefinition.geometry.coordinates[1].Length;
            for (var i = 0; i < InteriorCoordinatesLength; i++)
            {
                InteriorCoordinates.Set(i, new Vector2(m_trackDefinition.geometry.coordinates[1][i][0], m_trackDefinition.geometry.coordinates[1][i][1]));
            }
        }

        if (m_trackDefinition.geometry.coordinates[2].Length > 0)
        {
            StartlineCoordinatesLength = m_trackDefinition.geometry.coordinates[2].Length;
            for (var i = 0; i < StartlineCoordinatesLength; i++)
            {
                StartlineCoordinates.Set(i, new Vector2(m_trackDefinition.geometry.coordinates[2][i][0], m_trackDefinition.geometry.coordinates[2][i][1]));
            }
        }


    }

    public TrackDefinition GetTrackDefinition(NetworkArray<Vector2> _exteriorCoordinates, NetworkArray<Vector2> _interiorCoordinates, NetworkArray<Vector2> _startlineCoordinates, int _exteriorCoordinatesLength, int _interiorCoordinatesLength, int _startlineCoordinatesLength)
    {
        TrackDefinition trackDefinition = new TrackDefinition();
        trackDefinition.geometry = new Geometry();
        trackDefinition.properties = new Properties();

        var elrpc = _exteriorCoordinatesLength;
        var ilrpc = _interiorCoordinatesLength;
        var slrpc = _startlineCoordinatesLength;

        trackDefinition.geometry.coordinates = new float[3][][] {
            new float[elrpc][],
            new float[ilrpc][],
            new float[slrpc][],
        };


        try
        {
            if (_exteriorCoordinatesLength > 0)
            {
                for (var i = 0; i < _exteriorCoordinatesLength; i++)
                {
                    trackDefinition.geometry.coordinates[0][i] = new float[2];
                    trackDefinition.geometry.coordinates[0][i][0] = _exteriorCoordinates[i].x;
                    trackDefinition.geometry.coordinates[0][i][1] = _exteriorCoordinates[i].y;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }

        if (_interiorCoordinatesLength > 0)
        {
            for (var i = 0; i < _interiorCoordinatesLength; i++)
            {
                trackDefinition.geometry.coordinates[1][i] = new float[2];
                trackDefinition.geometry.coordinates[1][i][0] = _interiorCoordinates[i].x;
                trackDefinition.geometry.coordinates[1][i][1] = _interiorCoordinates[i].y;
            }
        }

        if (_startlineCoordinatesLength > 0)
        {
            for (var i = 0; i < _startlineCoordinatesLength; i++)
            {
                trackDefinition.geometry.coordinates[2][i] = new float[2];
                trackDefinition.geometry.coordinates[2][i][0] = _startlineCoordinates[i].x;
                trackDefinition.geometry.coordinates[2][i][1] = _startlineCoordinates[i].y;
            }
        }

        return trackDefinition;


    }

    void ITrackAPI.OnTrackDefinitionReceived(TrackDefinition trackDefinition)
    {
        m_trackDefinition = trackDefinition;
        OnTrackDefinitionUpdate(m_trackDefinition);
    }

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);

        if (Runner.IsServer)
        {
            if (TrackId == null) return;
            try
            {
                TrackId = Runner.SessionInfo.Properties["trackid"];
            }
            catch (System.Exception e)
            {
                Debug.Log($"TrackId not found in Runner.SessionInfo.Properties[\"trackid\"] : {e.Message}");
            }

            RestClientTrackGenerator.RegisterGetTrackDefinitionCompleteListener(this);

            try
            {
                RestClientTrackGenerator.GetTrackDefinition(TrackId);

            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }

    }

    /// <summary>
    /// Fixed update network.
    /// </summary>
    public override void FixedUpdateNetwork()
    {

        foreach (string propertyName in _changes.DetectChanges(this))
        {
            switch (propertyName)
            {
                case nameof(ExteriorCoordinates):
                    if (m_trackDefinition != null)
                    {
                        RenderTrack(m_trackDefinition, transform.localScale);
                    }

                    break;
                case nameof(TrackId):
                    try
                    {
                        RestClientTrackGenerator.GetTrackDefinition(TrackId);

                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                    break;

            }
        }

    }

    public override void Render()
    {
        if (!trackIsRendered && ExteriorCoordinatesLength > 0)
        {
            RenderTrack(GetTrackDefinition(ExteriorCoordinates, InteriorCoordinates, StartlineCoordinates, ExteriorCoordinatesLength, InteriorCoordinatesLength, StartlineCoordinatesLength), transform.localScale);
        }
    }

    private void RenderTrack(TrackDefinition trackDefinition, Vector3 scale)
    {
        scale = new Vector3(1f, 1f, 1f);
        m_exteriorLineRenderer = TrackExterior.GetComponent<LineRenderer>();
        m_exteriorLineRenderer.startWidth = .05f;
        m_exteriorLineRenderer.endWidth = .05f;

        m_exteriorLineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        m_exteriorLineRenderer.positionCount = trackDefinition.geometry.coordinates[0].Length;

        for (var i = 0; i < trackDefinition.geometry.coordinates[0].Length; i++)
        {
            var point = trackDefinition.geometry.coordinates[0][i];
            if (point != null) m_exteriorLineRenderer.SetPosition(i, new Vector3(point[0] * scale.x, (point[1]) * scale.y, 0f));
            new WaitForSeconds(renderDelay);
        }

        m_interiorLineRenderer = TrackInterior.GetComponent<LineRenderer>();
        m_interiorLineRenderer.startWidth = .05f;
        m_interiorLineRenderer.endWidth = .05f;

        m_interiorLineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        m_interiorLineRenderer.positionCount = trackDefinition.geometry.coordinates[1].Length;

        for (var i = 0; i < trackDefinition.geometry.coordinates[1].Length; i++)
        {
            var point = trackDefinition.geometry.coordinates[1][i];
            if (point != null)
            {
                m_interiorLineRenderer.SetPosition(i, new Vector3(point[0] * scale.x, point[1] * scale.y, 0f));
                new WaitForSeconds(renderDelay);
            }
        }

        if (trackDefinition.geometry.coordinates.Length < 3)
        {
            TrackStartLine.SetActive(false);
            return;
        }

        try
        {
            TrackStartLine.SetActive(true);
            m_startLineRenderer = TrackStartLine.GetComponent<LineRenderer>();
            m_startLineRenderer.startWidth = .05f;
            m_startLineRenderer.endWidth = .05f;

            m_startLineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            m_startLineRenderer.positionCount = trackDefinition.geometry.coordinates[2].Length;

            for (var i = 0; i < trackDefinition.geometry.coordinates[2].Length; i++)
            {
                var point = trackDefinition.geometry.coordinates[2][i];
                if (point != null) m_startLineRenderer.SetPosition(i, new Vector3(point[0] * scale.x, point[1] * scale.y, 0f));
                new WaitForSeconds(renderDelay);

            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }


        trackIsRendered = true;

    }

    
}
