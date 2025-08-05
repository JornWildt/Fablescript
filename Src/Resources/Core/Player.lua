PlayerPrototype = create_prototype(BaseObject)

Player = PlayerPrototype:new()
Player.visited_locations = {}

function Player:move_to(location)
  print("PLAYER MOVE")
  BaseObject.move_to(self, location)
  Player.visited_locations = location.name
end
