using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map_Generator
{
    public class GridGenerator
    {
        private readonly int tileSize = 40;
        private readonly int bossTileCount = 3;
        private readonly int itemTileCount = 2;
        private readonly int mapWidth = 5;
        private readonly int mapHeight = 5;

        private readonly Random rnd;
        private readonly LinkedList list;

        public int[,] tiles;
        List<Room> rooms;

        private Texture2D startTile;
        private Texture2D endTile;
        private Texture2D bossTile;
        private Texture2D defaultTile;
        private Texture2D itemTile;
        private Texture2D room1Texture;
        private Texture2D room2Texture;
        private Texture2D room2Texturet2;
        private Texture2D room3Texture;
        private Texture2D room4Texture;

        public enum TileType
        {
            def,
            start,
            end,
            item,
            boss,
        }

        public GridGenerator()
        {
            rnd = new Random();
            list = new LinkedList(mapWidth, mapHeight);
            rooms = new List<Room>();
        }

        public void LoadContent(Game game)
        {
            startTile = game.Content.Load<Texture2D>("tiles/gun");
            endTile = game.Content.Load<Texture2D>("tiles/health");
            defaultTile = game.Content.Load<Texture2D>("tiles/sword");
            itemTile = game.Content.Load<Texture2D>("tiles/itemDisplayBG");
            bossTile = game.Content.Load<Texture2D>("tiles/speed");
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
            for (int i = 0; i < itemTileCount; i++)
                PlaceTileAtRandom(TileType.item);

            //boss tiles
            for (int i = 0; i < bossTileCount; i++)
                PlaceTileAtRandom(TileType.boss);

            //find random path between start/finish
            List<Vector2> pathValues = list.GeneratePath(GetCorner(startTile), GetCorner(endTile));
            pathValues.Add(GetCorner(endTile));

            int[,] pathPlaces = new int[mapWidth, mapHeight];
            for (int i = 0; i < pathValues.Count; i++)
            {
                pathPlaces[(int)pathValues[i].X, (int)pathValues[i].Y] = 1;
            }

            //turn path values into rooms
            for (int i = 0; i < pathValues.Count - 1; i++)
            {
                Room.Direction direction1 = Room.Direction.DOWN;

                if (pathValues[i + 1].X > pathValues[i].X) direction1 = Room.Direction.RIGHT;
                else if (pathValues[i + 1].X < pathValues[i].X) direction1 = Room.Direction.LEFT;
                else if (pathValues[i + 1].Y < pathValues[i].Y) direction1 = Room.Direction.UP;

                if (i == 0) rooms.Add(new Room(pathValues[i], direction1));
                else rooms[i].AddDirection(direction1);

                rooms.Add(new Room(pathValues[i + 1], GetOpposite(direction1)));
            }


            List<Vector2> emptyRooms = new List<Vector2>();
            for (int x = 0; x < pathPlaces.GetLength(0); x++)
            {
                for (int y = 0; y < pathPlaces.GetLength(1); y++)
                {
                    if (pathPlaces[x, y] == 0)
                        emptyRooms.Add(new Vector2(x, y));
                }
            }

            //link up remaining rooms
            while (rooms.Count < (mapWidth * mapHeight))
            {
                for (int i = 0; i < emptyRooms.Count; i++)
                {
                    Vector2 roomPos = emptyRooms[i];
                    List<Room.Direction> availableDirections = new List<Room.Direction>();
                    if (roomPos.X > 0 && roomPos.Y > 0 && roomPos.X < mapWidth - 1 && roomPos.Y < mapHeight - 1)
                    {
                        if (pathPlaces[(int)roomPos.X - 1, (int)roomPos.Y] == 1)
                            availableDirections.Add(Room.Direction.LEFT);
                        if (pathPlaces[(int)roomPos.X + 1, (int)roomPos.Y] == 1)
                            availableDirections.Add(Room.Direction.RIGHT);
                        if (pathPlaces[(int)roomPos.X, (int)roomPos.Y - 1] == 1)
                            availableDirections.Add(Room.Direction.UP);
                        if (pathPlaces[(int)roomPos.X, (int)roomPos.Y + 1] == 1)
                            availableDirections.Add(Room.Direction.DOWN);
                    }
                    else
                    {
                        //if (room.position != GetCorner(startTile) && room.position != GetCorner(endTile))
                        //{
                        if (roomPos.X == 0)
                        {
                            if (pathPlaces[(int)roomPos.X + 1, (int)roomPos.Y] == 1)
                                availableDirections.Add(Room.Direction.RIGHT);
                            if (roomPos.Y == 0)
                            {
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y + 1] == 1)
                                    availableDirections.Add(Room.Direction.DOWN);
                            }
                            else if (roomPos.Y == mapHeight - 1)
                            {
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y - 1] == 1)
                                    availableDirections.Add(Room.Direction.UP);
                            }
                            else
                            {
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y - 1] == 1)
                                    availableDirections.Add(Room.Direction.UP);
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y + 1] == 1)
                                    availableDirections.Add(Room.Direction.DOWN);
                            }
                        }
                        else if (roomPos.X == mapWidth - 1)
                        {
                            if (pathPlaces[(int)roomPos.X - 1, (int)roomPos.Y] == 1)
                                availableDirections.Add(Room.Direction.LEFT);
                            if (roomPos.Y == 0)
                            {
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y + 1] == 1)
                                    availableDirections.Add(Room.Direction.DOWN);
                            }
                            else if (roomPos.Y == mapHeight - 1)
                            {
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y - 1] == 1)
                                    availableDirections.Add(Room.Direction.UP);
                            }
                            else
                            {
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y - 1] == 1)
                                    availableDirections.Add(Room.Direction.UP);
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y + 1] == 1)
                                    availableDirections.Add(Room.Direction.DOWN);
                            }
                            //}
                        }
                        else
                        {
                            if(roomPos.Y == 0)
                            {
                                if (pathPlaces[(int)roomPos.X + 1, (int)roomPos.Y] == 1)
                                    availableDirections.Add(Room.Direction.RIGHT);
                                if (pathPlaces[(int)roomPos.X - 1, (int)roomPos.Y] == 1)
                                    availableDirections.Add(Room.Direction.LEFT);
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y + 1] == 1)
                                    availableDirections.Add(Room.Direction.DOWN);
                            }
                            else if(roomPos.Y == mapHeight- 1)
                            {
                                if (pathPlaces[(int)roomPos.X + 1, (int)roomPos.Y] == 1)
                                    availableDirections.Add(Room.Direction.RIGHT);
                                if (pathPlaces[(int)roomPos.X - 1, (int)roomPos.Y] == 1)
                                    availableDirections.Add(Room.Direction.LEFT);
                                if (pathPlaces[(int)roomPos.X, (int)roomPos.Y - 1] == 1)
                                    availableDirections.Add(Room.Direction.UP);
                            }
                        }
                    }
                    if (availableDirections.Count > 0)
                    {
                        Room.Direction direction = availableDirections[rnd.Next(availableDirections.Count)];
                        Vector2 nextRoomPos = roomPos;
                        if (direction == Room.Direction.DOWN) nextRoomPos.Y += 1;
                        else if (direction == Room.Direction.UP) nextRoomPos.Y -= 1;
                        else if (direction == Room.Direction.RIGHT) nextRoomPos.X += 1;
                        else if (direction == Room.Direction.LEFT) nextRoomPos.X -= 1;

                        rooms.Where(x => x.position == nextRoomPos).First().AddDirection(GetOpposite(direction));
                        rooms.Add(new Room(roomPos, direction));
                        pathPlaces[(int)roomPos.X, (int)roomPos.Y] = 1;
                        emptyRooms.Remove(roomPos);
                    }
                }
            }

            //add correct texture
            for (int i = 0; i < rooms.Count(); i++)
            {
                switch (rooms[i].directions.Count)
                {
                    case 1: rooms[i].SetTexture(room1Texture); break;
                    case 2:
                        if (rooms[i].GetRoomType() == Room.RoomType.HALL_HORIZONTAL || rooms[i].GetRoomType() == Room.RoomType.HALL_VETICAL)
                            rooms[i].SetTexture(room2Texturet2);
                        else rooms[i].SetTexture(room2Texture);
                        break;
                    case 3: rooms[i].SetTexture(room3Texture); break;
                    case 4: rooms[i].SetTexture(room4Texture); break;
                }
            }
        }

        private void PlaceTileAtRandom(TileType tile)
        {
            var xpos = rnd.Next(tiles.GetLength(0));
            var ypos = rnd.Next(tiles.GetLength(1));
            while (tiles[xpos, ypos] != 0)
            {
                xpos = rnd.Next(tiles.GetLength(0));
                ypos = rnd.Next(tiles.GetLength(1));
            }
            tiles[xpos, ypos] = (int)tile;
        }

        private Vector2 GetCorner(int cornerNum)
        {
            return cornerNum switch
            {
                0 => new Vector2(0, 0),
                1 => new Vector2(tiles.GetLength(0) - 1, 0),
                2 => new Vector2(0, tiles.GetLength(1) - 1),
                3 => new Vector2(tiles.GetLength(0) - 1, tiles.GetLength(1) - 1),
                _ => new Vector2(0, 0),
            };
        }

        public void DrawGrid(SpriteBatch spriteBatch)
        {
            for(int x = 0; x < tiles.GetLength(0); x++)
            {
                for(int y = 0; y < tiles.GetLength(1); y++)
                {
                    switch (tiles[x, y]) {
                        case (int)TileType.start:
                            spriteBatch.Draw(startTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);break;
                        case (int)TileType.end:
                            spriteBatch.Draw(endTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);break;
                        case (int)TileType.boss:
                            spriteBatch.Draw(bossTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);break;
                        case (int)TileType.item:
                            spriteBatch.Draw(itemTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);break;
                        default:
                            spriteBatch.Draw(defaultTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);break;
                    }
                }
            }

            for(int i = 0; i < rooms.Count; i++)
            {
                rooms[i].Draw(spriteBatch, tileSize);
            }
        }

        private Room.Direction GetOpposite(Room.Direction direction)
        {
            return direction switch
            {
                Room.Direction.RIGHT => Room.Direction.LEFT,
                Room.Direction.UP => Room.Direction.DOWN,
                Room.Direction.LEFT => Room.Direction.RIGHT,
                _ => Room.Direction.UP,
            };
        }

        private class LinkedList
        {
            private Node head;
            private int[,] visited;
            private readonly int mapW, mapH;

            public LinkedList(int mapW, int mapH)
            {
                this.mapW = mapW;
                this.mapH = mapH;
            }            

            public List<Vector2> GeneratePath(Vector2 startPosition, Vector2 endPosition)
            {
                visited = new int[mapW, mapH];
                head = new Node(startPosition, null);

                Random rnd = new Random();
                Node currentNode = head;

                Vector2 returnedFrom = currentNode.position;
                Vector2 nextPosition = currentNode.position;
                while (nextPosition != endPosition)
                {                    
                    List<Vector2> availablePositions = new List<Vector2>();
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
                            //path found + return
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
                    {//reset when hitting dead end
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
            public List<Direction> directions;

            private Texture2D texture;
            private float textureRotation;
            private RoomType roomType;

            public RoomType GetRoomType() { return roomType; }
            public void SetTexture(Texture2D newTexture) { texture = newTexture; }

            public enum Direction
            {
                UP,
                RIGHT,
                DOWN,
                LEFT
            }

            public enum RoomType
            {
                UP,
                DOWN,
                RIGHT,
                LEFT,
                UP_RIGHT,
                RIGHT_DOWN,
                DOWN_LEFT,
                LEFT_UP,
                UP_RIGHT_DOWN,
                RIGHT_DOWN_LEFT,
                DOWN_LEFT_UP,
                LEFT_UP_RIGHT,
                HALL_HORIZONTAL,
                HALL_VETICAL,
                ALL
            }

            public Room(Vector2 position, Direction direction)
            {
                directions = new List<Direction>();
                this.position = position;
                directions.Add(direction);
                ResetType();
            }

            private void ResetType()
            {
                if (directions.Count == 1)
                {
                    if (directions[0] == Direction.DOWN) { textureRotation = 0f; roomType = RoomType.DOWN; }
                    if (directions[0] == Direction.LEFT) { textureRotation = (float)Math.PI / 2; roomType = RoomType.LEFT; }
                    if (directions[0] == Direction.UP) { textureRotation = (float)Math.PI; ; roomType = RoomType.UP; }
                    if (directions[0] == Direction.RIGHT) { textureRotation = -(float)Math.PI / 2; ; roomType = RoomType.RIGHT; }
                }
                else if (directions.Count == 2)
                {
                    if (directions.Contains(Direction.DOWN) && directions.Contains(Direction.RIGHT)) { textureRotation = 0f; roomType = RoomType.RIGHT_DOWN; }
                    else if (directions.Contains(Direction.DOWN) && directions.Contains(Direction.LEFT)) { textureRotation = (float)Math.PI / 2; roomType = RoomType.DOWN_LEFT; }
                    else if (directions.Contains(Direction.LEFT) && directions.Contains(Direction.UP)) { textureRotation = (float)Math.PI; roomType = RoomType.LEFT_UP; }
                    else if (directions.Contains(Direction.UP) && directions.Contains(Direction.RIGHT)) { textureRotation = -(float)Math.PI / 2; roomType = RoomType.UP_RIGHT; }
                    else if (directions.Contains(Direction.LEFT) && directions.Contains(Direction.RIGHT)) { textureRotation = (float)Math.PI / 2; roomType = RoomType.HALL_HORIZONTAL; }
                    else if (directions.Contains(Direction.UP) && directions.Contains(Direction.DOWN)) { textureRotation = 0f; roomType = RoomType.HALL_VETICAL; }
                }
                else if (directions.Count == 3)
                {
                    if (directions.Contains(Direction.DOWN) && directions.Contains(Direction.RIGHT) && directions.Contains(Direction.UP)) { textureRotation = 0f; roomType = RoomType.UP_RIGHT_DOWN; }
                    else if (directions.Contains(Direction.DOWN) && directions.Contains(Direction.LEFT) && directions.Contains(Direction.RIGHT)) { textureRotation = (float)Math.PI / 2; roomType = RoomType.RIGHT_DOWN_LEFT; }
                    else if (directions.Contains(Direction.LEFT) && directions.Contains(Direction.UP) && directions.Contains(Direction.DOWN)) { textureRotation = (float)Math.PI; roomType = RoomType.DOWN_LEFT_UP; }
                    else if (directions.Contains(Direction.UP) && directions.Contains(Direction.RIGHT) && directions.Contains(Direction.LEFT)) { textureRotation = -(float)Math.PI / 2; roomType = RoomType.LEFT_UP_RIGHT; }
                }
                else if (directions.Count == 4) { textureRotation = 0f; roomType = RoomType.ALL; }
            }

            public void AddDirection(Direction direction)
            {
                directions.Add(direction);
                ResetType();
            }

            public void Draw(SpriteBatch spriteBatch, int tileSize)
            {
                spriteBatch.Draw(texture, new Rectangle(((int)position.X * tileSize) + tileSize/2, ((int)position.Y * tileSize) + tileSize/2, tileSize, tileSize), null, Color.White, textureRotation, new Vector2(texture.Width/2,texture.Height/2), SpriteEffects.None, 1.0f); 
            }
        }
    }        
}
