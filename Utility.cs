using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Helper class with frequently used things.
/// </summary>
public static class Utility {
    /// <summary>
    /// X offsets for 8 nearest cells (use only odds if you need 4 nearest cells).
    /// </summary>
    public static readonly int[] Dx = { -1, 0, 1, 1, 1, 0, -1, -1 };

    /// <summary>
    /// Y offsets for 8 nearest cells (use only odds if you need 4 nearest cells).
    /// </summary>
    public static readonly int[] Dy = { -1, -1, -1, 0, 1, 1, 1, 0 };

    public static readonly string Version = "TEST BUILD v0.1.3";

    public static string levelDirectory = Application.dataPath + "/";

    /// <summary>
    /// Check if <paramref name="coord"/> in range of <paramref name="minCoord"/>..<paramref name="maxCoord"/>.
    /// </summary>
    /// <param name="coord"> Coordinate to check. </param>
    /// <param name="minCoord"> Minimal coordinate. </param>
    /// <param name="maxCoord"> Maximal coordinate. </param>
    public static bool RangeCheck(int coord, int minCoord, int maxCoord) {
        return coord <= maxCoord && coord >= minCoord;
    }

    /// <summary>
    /// Rotate <paramref name="obj"/> to face <paramref name="pointOfView"/>.
    /// </summary>
    /// <param name="obj">Object to rotate.</param>
    /// <param name="pointOfView"> Point of direction. </param>
    public static void Rotate(GameObject obj, Vector2 pointOfView) {
        float angleRad = Mathf.Atan2(pointOfView.y - obj.transform.position.y, pointOfView.x - obj.transform.position.x);
        float angleDeg = (180 / Mathf.PI) * angleRad;

        obj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);
    }

    /// <summary>
    /// Clone matrix <paramref name="from"/> into matrix <paramref name="to"/>. <paramref name="from"/>
    /// capacity shoud be less or equal to <paramref name="to"/>.
    /// </summary>
    /// <param name="from"> Original matrix. </param>
    /// <param name="to"> Copy. </param>
    /// <typeparam name="T"> Type of matrix cell. </typeparam>
    public static void Clone<T>(T[,] from, T[,] to, int width, int height)
        where T : struct {
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                to[i, j] = from[i, j];
            }
        }
    }

    /// <summary>
    /// (DEBUG ONLY) Prints <paramref name="matrix"/> into Unity Debug stream.
    /// </summary>
    /// <param name="matrix"> Matrix to print. </param>
    /// <typeparam name="T"> Type of matrix cell. </typeparam>
    public static void PrintMatrix<T>(T[,] matrix, int width, int height)
        where T : struct {
        GameLogger.LogMessage("=========================", "MatrixPrint");
        for (int i = 0; i < height; i++) {
            string buffer = string.Empty;
            for (int j = 0; j < width; j++) {
                buffer += Convert.ToInt32(matrix[i, j]);
            }

            GameLogger.LogMessage(buffer, "MatrixPrint");
        }

        GameLogger.LogMessage("=========================", "MatrixPrint");
    }

    /// <summary>
    /// Selects random item from <paramref name="list"/>.
    /// </summary>
    /// <typeparam name="T"> Type of list. </typeparam>
    /// <param name="list"> List from which item will be selected. </param>
    /// <returns> Item from <paramref name="list"/>. </returns>
    public static T SelectRandomItem<T>(List<T> list) {
        return list[Random.Range(0, list.Count)];
    }
}
