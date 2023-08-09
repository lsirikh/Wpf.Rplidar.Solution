using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Wpf.Rplidar.Solution.Utils
{
    /****************************************************************************
        Purpose      :                                                           
        Created By   : GHLee                                                
        Created On   : 7/31/2023 4:32:01 PM                                                    
        Department   : SW Team                                                   
        Company      : Sensorway Co., Ltd.                                       
        Email        : lsirikh@naver.com                                         
     ****************************************************************************/

    public class ConnectedComponentFinder
    {

        #region - Ctors -
        #endregion
        #region - Implementation of Interface -
        #endregion
        #region - Overrides -
        #endregion
        #region - Binding Methods -
        #endregion
        #region - Processes -
        // 주어진 점들의 목록을 받아서, 연결된 구성 요소들의 목록을 반환합니다.
        public List<List<Point>> GetConnectedComponents(List<Point> points, double threshold)
        {
            var visited = new HashSet<Point>(new PointComparer());  // 방문한 점들의 집합
            var connectedComponents = new List<List<Point>>();  // 연결된 구성 요소들의 목록

            foreach (var point in points)
            {
                if (!visited.Contains(point))  // 방문하지 않은 점에 대해서만 처리
                {
                    var connectedComponent = GetConnectedComponent(point, points, threshold, visited);
                    connectedComponents.Add(connectedComponent);  // 연결된 구성 요소를 목록에 추가
                }
            }

            return connectedComponents;  // 연결된 구성 요소들의 목록을 반환
        }

        // 시작점부터 DFS를 사용하여 연결된 구성 요소를 반환합니다.
        private List<Point> GetConnectedComponent(Point start, List<Point> points, double threshold, HashSet<Point> visited)
        {
            var stack = new Stack<Point>();  // DFS에서 사용하는 스택
            var connectedComponent = new List<Point>();  // 현재 구성 요소의 점들

            stack.Push(start);  // 시작점을 스택에 추가

            while (stack.Count != 0)
            {
                var currentPoint = stack.Pop();  // 스택에서 점을 하나 꺼냄

                if (visited.Contains(currentPoint))  // 이미 방문한 점이라면 무시
                    continue;

                visited.Add(currentPoint);  // 방문한 점을 집합에 추가
                connectedComponent.Add(currentPoint);  // 현재 구성 요소에 점을 추가

                var neighbours = GetNeighbours(currentPoint, points, threshold);  // 이웃 점들을 가져옴

                foreach (var neighbour in neighbours)
                    if (!visited.Contains(neighbour))  // 방문하지 않은 이웃 점만 스택에 추가
                        stack.Push(neighbour);
            }

            return connectedComponent;  // 연결된 구성 요소를 반환
        }

        // 주어진 점의 이웃 점들을 반환합니다.
        private List<Point> GetNeighbours(Point point, List<Point> points, double threshold)
        {
            return points
                .Where(p => IsConnected(point, p, threshold))  // 주어진 점과 연결된 점만 반환
                .ToList();
        }

        // 두 점이 연결되어 있는지 판단합니다.
        // 거리가 주어진 임계값 이하면 연결되어 있다고 판단합니다.
        private bool IsConnected(Point p1, Point p2, double threshold)
        {
            double distance = (p1 - p2).Length;  // 두 점 사이의 거리
            return distance <= threshold;
        }
        #endregion
        #region - IHanldes -
        #endregion
        #region - Properties -
        #endregion
        #region - Attributes -
        #endregion
    }
}
