Commands = {}


function Commands.inspect(objName)
  print('INSPECT: ' .. objName)
  local obj = Objects[objName];
  obj:inspect()
end


function Commands.take(objName)
  print('TAKE: ' .. objName)
  local obj = Objects[objName];
  obj:inspect()
end


function Commands.look_at(obj)
  print("You see: " .. obj.name)
end


function Commands.drop_object(obj)
	obj.move_to(Player.location)
end

