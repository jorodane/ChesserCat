using System.Linq;
using UnityEngine;

public class GuideLine : MonoBehaviour
{
	[SerializeField] LineRenderer _line;
	[SerializeField] LineRenderer _head;
	[SerializeField] SpriteRenderer _sprite;

	[SerializeField] float headLength = 0.25f;

	Vector3Int startPosition;
	Vector3Int endPosition;

	public void SetStart(Vector3Int position)
	{
		startPosition = position;
		Vector3 worldPosition = TileManager.GetTileWorldPosition(position);
		transform.position = worldPosition;
		_line.positionCount = 1;
		_line.SetPositions(new Vector3[] { worldPosition });
		RendererSwitcher(1);
	}

	public void SetEnd(Vector3Int position)
	{
		endPosition = position;
		Vector3[] path = TileManager.GetTilePathPositions(startPosition, endPosition).ToArray();
		int pathCount = path.Length;
		if(pathCount >= 2)
		{
			Vector3 lastPosition = path[pathCount - 1];
			Vector3 lastDirection = (lastPosition - path[pathCount - 2]).normalized * headLength;
			lastPosition = path[pathCount - 1] -= lastDirection;
			_line.positionCount = pathCount;
			_line.SetPositions(path);
			_head.SetPosition(0, lastPosition);
			_head.SetPosition(1, lastPosition + lastDirection);
			transform.position = path[0];
		}
		else
		{
			_line.positionCount = 0;
		}
		RendererSwitcher(pathCount);
	}

	public void RendererSwitcher(int positionCount)
	{
		if(positionCount <= 1)
		{
			_head.enabled = _line.enabled = false;
			_sprite.enabled = true;
		}
		else
		{
			_head.enabled = _line.enabled = true;
			_sprite.enabled = false;
		}
	}

}
