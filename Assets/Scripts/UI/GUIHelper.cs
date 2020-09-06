using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GUIHelper {
	private static Dictionary<Color, Texture2D> m_colourTextureCache = new Dictionary<Color, Texture2D>();

	public static Texture2D GetColourTexture(Color color) {
		Texture2D result;
		if (m_colourTextureCache.TryGetValue(color, out result)) {
			return result;
		}

		result = new Texture2D(1, 1);
		result.SetPixel(0, 0, color);
		result.Apply();

		m_colourTextureCache.Add(color, result);
		return result;
	}
}
