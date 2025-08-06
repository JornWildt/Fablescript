PlayerPrototype = create_prototype(BaseObject)

Player = PlayerPrototype:new()
Player.described_locations = {}

function Player:move_to(location)
  print("PLAYER MOVE")
  BaseObject.move_to(self, location)
end
