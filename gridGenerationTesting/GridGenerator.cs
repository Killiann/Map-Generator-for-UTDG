using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridGenerationTesting
{
    public class GridGenerator
    {
        public int[,] tiles;
        private int tileSize = 30;
        Random rnd;
        LinkedList list;
        int mapWidth;
        int mapHeight;

        List<Room> rooms;

        public enum TileType
        {
            none,
            start,
            end,
            item,
            path,
            boss,
            normal
        }
        private Texture2D startTile;
        private Texture2D endTile;
        private Texture2D pathTile;
        private Texture2D defaultTile;
        private Texture2D itemTile;

        private Texture2D room1Texture;
        private Texture2D room2Texture;
        private Texture2D room2Texturet2;
        private Texture2D room3Texture;
        private Texture2D room4Texture;

        public GridGenerator()
        {
            rnd = new Random();
            mapWidth = 4;
            mapHeight = 4;
            list = new LinkedList(mapWidth, mapHeight);
            rooms = new List<Room>();
        }
        public void LoadContent(Game game)
        {
            startTile = game.Content.Load<Texture2D>("tiles/startTile");
            endTile = game.Content.Load<Texture2D>("tiles/endTile");
            pathTile = game.Content.Load<Texture2D>("tiles/pathTile");
            defaultTile = game.Content.Load<Texture2D>("tiles/tileMap");
            itemTile = game.Content.Load<Texture2D>("tiles/health");

            room1Texture = game.Content.Load<Texture2D>("rooms/room_1open");
            room2Texture = game.Content.Load<Texture2D>("rooms/room_2open");
            room2Texturet2 = game.Content.Load<Texture2D>("rooms/room_2openv2");
            room3Texture = game.Content.Load<Texture2D>("rooms/room_3open");
            room4Texture = game.Content.Load<Texture2D>("rooms/room_4open");
        }

        public void GenerateGrid()
        {
            tiles = new int[mapWidth, mapHeight];
            rooms = new List<Room>();

            //spawn start/ end in the corners
            int startTile = rnd.Next(4);
            int endTile = rnd.Next(4);
            while (endTile == startTile)
                endTile = rnd.Next(4);

            tiles[(int)GetCorner(startTile).X, (int)GetCorner(startTile).Y] = (int)TileType.start;
            tiles[(int)GetCorner(endTile).X, (int)GetCorner(endTile).Y] = (int)TileType.end;

            //item tiles
            for (int i = 0; i < 2; i++)
            {
                var xpos = rnd.Next(tiles.GetLength(0));
                var ypos = rnd.Next(tiles.GetLength(1));
                while (tiles[xpos, ypos] != 0)
                {
                    xpos = rnd.Next(tiles.GetLength(0));
                    ypos = rnd.Next(tiles.GetLength(1));
                }
                tiles[xpos, ypos] = (int)TileType.item;
            }

            //boss tiles
            for (int i = 0; i < 3; i++)
            {
                var xpos = rnd.Next(tiles.GetLength(0));
                var ypos = rnd.Next(tiles.GetLength(1));
                while (tiles[xpos, ypos] != 0)
                {
                    xpos = rnd.Next(tiles.GetLength(0));
                    ypos = rnd.Next(tiles.GetLength(1));
                }
                tiles[xpos, ypos] = (int)TileType.boss;
            }

            //2: find random path between the two
            List<Vector2> pathValues = list.generatePath(GetCorner(startTile), GetCorner(endTile), tiles);
            pathValues.Add(GetCorner(endTile));

            int[,] pathPlaces = new int[mapWidth, mapHeight];
            for (int i = 0; i < pathValues.Count; i++)
            {
                pathPlaces[(int)pathValues[i].X, (int)pathValues[i].Y] = 1;
            }


            for (int i = 0; i < pathValues.Count - 1; i++)
            {
                if (i == 0)
                {
                    Room.Direction direction1 = Room.Direction.DOWN;
                    Room.Direction direction2 = Room.Direction.UP;
                    if (pathValues[i + 1].X > pathValues[i].X) {
                        direction1 = Room.Direction.RIGHT;
                        direction2 = Room.Direction.LEFT;
                    }
                    else if (pathValues[i + 1].X < pathValues[i].X) {
                        direction1 = Room.Direction.LEFT;
                        direction2 = Room.Direction.RIGHT;
                    }
                    else if (pathValues[i + 1].Y < pathValues[i].Y) {
                        direction1 = Room.Direction.UP;
                        direction2 = Room.Direction.DOWN;
                    }
                    rooms.Add(new Room(pathValues[i], direction1));
                    rooms.Add(new Room(pathValues[i + 1], direction2));
                }
                else
                {
                    Room.Direction direction1 = Room.Direction.DOWN;
                    Room.Direction direction2 = Room.Direction.UP;
                    if (pathValues[i + 1].X > pathValues[i].X)
                    {
                        direction1 = Room.Direction.RIGHT;
                        direction2 = Room.Direction.LEFT;
                    }
                    else if (pathValues[i + 1].X < pathValues[i].X)
                    {
                        direction1 = Room.Direction.LEFT;
                        direction2 = Room.Direction.RIGHT;
                    }
                    else if (pathValues[i + 1].Y < pathValues[i].Y)
                    {
                        direction1 = Room.Direction.UP;
                        direction2 = Room.Direction.DOWN;
                    }
                    rooms[i].AddDirection(direction1);
                    rooms.Add(new Room(pathValues[i + 1], direction2));
                }
            }

            while (rooms.Count < (mapWidth * mapHeight))
            {
                for (int r = 0; r < rooms.Count; r++) {
                    Room room = rooms[r];
                    if (room.position.X > 0 && room.position.Y > 0 && room.position.X < mapWidth - 1 && room.position.Y < mapHeight - 1)
                    {
                        List<Room.Direction> availableDirections = new List<Room.Direction>();
                        if (pathPlaces[(int)room.position.X - 1, (int)room.position.Y] == 0)
                            availableDirections.Add(Room.Direction.LEFT);
                        else if (pathPlaces[(int)room.position.X + 1, (int)room.position.Y] == 0)
                            availableDirections.Add(Room.Direction.RIGHT);
                        else if (pathPlaces[(int)room.position.X, (int)room.position.Y - 1] == 0)
                            availableDirections.Add(Room.Direction.UP);
                        else if (pathPlaces[(int)room.position.X, (int)room.position.Y + 1] == 0)
                            availableDirections.Add(Room.Direction.DOWN);

                        Room.Direction direction = (Room.Direction)rnd.Next(availableDirections.Count);
                        Vector2 newPosition = room.position;
                        if (direction == Room.Direction.DOWN) newPosition.Y += 1;
                        if (direction == Room.Direction.UP) newPosition.Y -= 1;
                        if (direction == Room.Direction.RIGHT) newPosition.X += 1;
                        if (direction == Room.Direction.LEFT) newPosition.Y -= 1;

                        rooms.Add(new Room(newPosition, direction));
                        pathPlaces[(int)newPosition.X, (int)newPosition.Y] = 1;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (pathPlaces[(int)GetCorner(i).X, (int)GetCorner(i).Y] == 0)
                    {
                        if (GetCorner(i).X == 0)
                        {
                            if (GetCorner(i).Y == 0)
                            {
                                int d = rnd.Next(2);
                                if (d == 0) rooms.Add(new Room(GetCorner(i), Room.Direction.RIGHT));
                                else if (d == 1) rooms.Add(new Room(GetCorner(i), Room.Direction.DOWN));
                            }
                            else
                            {
                                int d = rnd.Next(2);
                                if (d == 0) rooms.Add(new Room(GetCorner(i), Room.Direction.RIGHT));
                                else if (d == 1) rooms.Add(new Room(GetCorner(i), Room.Direction.UP));
                            }
                        } else
                        {
                            if (GetCorner(i).Y == 0)
                            {
                                int d = rnd.Next(2);
                                if (d == 0) rooms.Add(new Room(GetCorner(i), Room.Direction.LEFT));
                                else if (d == 1) rooms.Add(new Room(GetCorner(i), Room.Direction.DOWN));
                            }
                            else
                            {
                                int d = rnd.Next(2);
                                if (d == 0) rooms.Add(new Room(GetCorner(i), Room.Direction.LEFT));
                                else if (d == 1) rooms.Add(new Room(GetCorner(i), Room.Direction.UP));
                            }
                        }
                    }
                }
            }        

            for(int i = 0; i < rooms.Count(); i++)
            {
                if(rooms[i].directions.Count == 1)
                {
                    if (rooms[i].directions[0] == Room.Direction.DOWN)
                        rooms[i].SetTexture(room1Texture, 0f);
                    else if (rooms[i].directions[0] == Room.Direction.LEFT)
                        rooms[i].SetTexture(room1Texture, (float)Math.PI / 2);
                    else if (rooms[i].directions[0] == Room.Direction.UP)
                        rooms[i].SetTexture(room1Texture, (float)Math.PI);
                    else if (rooms[i].directions[0] == Room.Direction.RIGHT)
                        rooms[i].SetTexture(room1Texture, -(float)Math.PI / 2);
                }
                else if(rooms[i].directions.Count == 2)
                {
                    if (rooms[i].directions.Contains(Room.Direction.DOWN) && rooms[i].directions.Contains(Room.Direction.RIGHT))
                        rooms[i].SetTexture(room2Texture, 0f);
                    else if (rooms[i].directions.Contains(Room.Direction.DOWN) && rooms[i].directions.Contains(Room.Direction.LEFT))
                        rooms[i].SetTexture(room2Texture, (float)Math.PI / 2);
                    else if (rooms[i].directions.Contains(Room.Direction.LEFT) && rooms[i].directions.Contains(Room.Direction.UP))
                        rooms[i].SetTexture(room2Texture, (float)Math.PI);
                    else  if (rooms[i].directions.Contains(Room.Direction.UP) && rooms[i].directions.Contains(Room.Direction.RIGHT))
                        rooms[i].SetTexture(room2Texture, -(float)Math.PI / 2);

                    else if (rooms[i].directions.Contains(Room.Direction.LEFT) && rooms[i].directions.Contains(Room.Direction.RIGHT))
                        rooms[i].SetTexture(room2Texturet2, (float)Math.PI/2);
                    else if (rooms[i].directions.Contains(Room.Direction.UP) && rooms[i].directions.Contains(Room.Direction.DOWN))
                        rooms[i].SetTexture(room2Texturet2, 0f);
                }else if(rooms[i].directions.Count == 3)
                {
                    if (rooms[i].directions.Contains(Room.Direction.DOWN) && rooms[i].directions.Contains(Room.Direction.RIGHT) && rooms[i].directions.Contains(Room.Direction.UP))
                        rooms[i].SetTexture(room3Texture, 0f);
                    else if (rooms[i].directions.Contains(Room.Direction.DOWN) && rooms[i].directions.Contains(Room.Direction.LEFT) && rooms[i].directions.Contains(Room.Direction.RIGHT))
                        rooms[i].SetTexture(room3Texture, (float)Math.PI / 2);
                    else if (rooms[i].directions.Contains(Room.Direction.LEFT) && rooms[i].directions.Contains(Room.Direction.UP) && rooms[i].directions.Contains(Room.Direction.DOWN))
                        rooms[i].SetTexture(room3Texture, (float)Math.PI);
                    else if (rooms[i].directions.Contains(Room.Direction.UP) && rooms[i].directions.Contains(Room.Direction.RIGHT) && rooms[i].directions.Contains(Room.Direction.LEFT))
                        rooms[i].SetTexture(room3Texture, -(float)Math.PI / 2);
                }
                else if(rooms[i].directions.Count == 4)
                {
                    rooms[i].SetTexture(room4Texture, 0f);
                }
            }            
        }        

        private Vector2 GetCorner(int cornerNum)
        {            
            switch (cornerNum)
            {
                case 0: return new Vector2(0, 0);
                case 1: return new Vector2(tiles.GetLength(0)-1, 0);
                case 2: return new Vector2(0, tiles.GetLength(1)-1);
                case 3: return new Vector2(tiles.GetLength(0)-1, tiles.GetLength(1)-1);
            }
            return new Vector2(0,0);
        }

        public void DrawGrid(SpriteBatch spriteBatch)
        {
            for(int x = 0; x < tiles.GetLength(0); x++)
            {
                for(int y = 0; y < tiles.GetLength(1); y++)
                {
                    switch (tiles[x, y]) {
                        case (int)TileType.start:
                            spriteBatch.Draw(startTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
                            break;
                        case (int)TileType.end:
                            spriteBatch.Draw(endTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
                            break;
                        case (int)TileType.boss:
                            spriteBatch.Draw(pathTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
                            break;
                        case (int)TileType.item:
                            spriteBatch.Draw(itemTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
                            break;
                        default:
                            spriteBatch.Draw(defaultTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
                            break;
                    }
                }
            }

            for(int i = 0; i < rooms.Count; i++)
            {
                rooms[i].Draw(spriteBatch);
            }
        }


        private class LinkedList
        {
            Node head;
            Vector2 endPosition;
            int[,] visited;
            int mapW, mapH;

            public LinkedList(int mapW, int mapH)
            {
                this.mapW = mapW;
                this.mapH = mapH;
            }            

            public List<Vector2> generatePath(Vector2 startPosition, Vector2 endPosition, int[,] takenPositions)
            {
                //setup
                visited = new int[mapW, mapH];
                int[,] baseSetup = new int[mapW, mapH];
                head = new Node(startPosition, null);
                this.endPosition = endPosition;

                Random rnd = new Random();
                Node currentNode = head;

                Vector2 returnedFrom = currentNode.position;
                Vector2 nextPosition = currentNode.position;
                while (nextPosition != endPosition)
                {                    
                    List<Vector2> availablePositions = new List<Vector2>();
                    //check available         
                    //left
                    if (currentNode.position.X != 0 && visited[(int)currentNode.position.X - 1, (int)currentNode.position.Y] == 0)
                        availablePositions.Add(new Vector2(currentNode.position.X - 1, currentNode.position.Y));
                    //right
                    if (currentNode.position.X != mapW-1 && visited[(int)currentNode.position.X + 1, (int)currentNode.position.Y] == 0)
                        availablePositions.Add(new Vector2(currentNode.position.X + 1, currentNode.position.Y));
                    //up
                    if (currentNode.position.Y != 0 && visited[(int)currentNode.position.X, (int)currentNode.position.Y - 1] == 0)
                        availablePositions.Add(new Vector2(currentNode.position.X, currentNode.position.Y - 1));
                    //down
                    if (currentNode.position.Y != mapH-1 && visited[(int)currentNode.position.X, (int)currentNode.position.Y + 1] == 0)
                        availablePositions.Add(new Vector2(currentNode.position.X, currentNode.position.Y + 1));

                    if (availablePositions.Contains(returnedFrom)) availablePositions.Remove(returnedFrom);

                    if (availablePositions.Count > 0)
                    {
                        nextPosition = availablePositions[rnd.Next(availablePositions.Count)];

                        if (nextPosition != endPosition)
                        {
                            currentNode.next = new Node(nextPosition, currentNode);
                            visited[(int)currentNode.position.X, (int)currentNode.position.Y] = 1;
                            currentNode = currentNode.next;
                        }
                        else
                        {
                            currentNode.next = new Node(nextPosition, currentNode);
                            currentNode = head;
                            List<Vector2> pathPositions = new List<Vector2>();
                            while (currentNode.next != null)
                            {
                                pathPositions.Add(currentNode.position);
                                currentNode = currentNode.next;
                            }
                            return pathPositions;
                        }
                    }
                    else
                    {
                        head = new Node(startPosition, null);
                        currentNode = head;
                        returnedFrom = currentNode.position;
                        nextPosition = currentNode.position;
                        visited = new int[mapW, mapH];
                    }
                }
                return null;
            }
        }

        private class Node
        {
            public Vector2 position;
            public Node prev;
            public Node next;
            public Node(Vector2 position, Node prev)
            {
                this.position = position;
                this.prev = prev;
            }
        }

        private class Room
        {
            public Vector2 position;
            private Texture2D texture;
            private float textureRotation;

            public enum Direction
            {
                UP,
                RIGHT,
                DOWN,
                LEFT
            }

            public List<Direction> directions;

            public Room(Vector2 position, Direction direction)
            {
                directions = new List<Direction>();
                this.position = position;
                directions.Add(direction);
            }

            public void SetTexture(Texture2D newTexture, float rotation)
            {
                texture = newTexture;
                textureRotation = rotation;
            }

            public void AddDirection(Direction direction)
            {
                directions.Add(direction);
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(texture, new Rectangle(((int)position.X * 30)+15, ((int)position.Y * 30)+15, 30, 30), null, Color.White, textureRotation, new Vector2(texture.Width/2,texture.Height/2), SpriteEffects.None, 1.0f); 
            }
        }
    }        
}
