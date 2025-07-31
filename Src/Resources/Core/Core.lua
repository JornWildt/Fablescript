-- Player


-- Commands
Commands = {}


function Commands.look_at(obj)
  print("You see: " .. obj.name)
end


function Commands.drop_object(obj)
	obj.move_to(Player.location)
end

