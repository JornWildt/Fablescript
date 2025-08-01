Commands = {}


function Commands.inspect(objName)
  print('INSPECT: ' .. objName)
  local obj = Objects[objName];
  if obj then
    Core.say("'" .. obj.name .. "' is at '" .. ((obj.location and obj.location.name) or "<nowhere>") .. "'")
    for i, v in ipairs(obj.objects_here) do
      Core.say("  '" .. v.name .. "' is here")
    end
  else
    Core.say("Unknown object.");
  end
end


function Commands.take(objName)
  print('TAKE: ' .. objName)
  local obj = Objects[objName];
  obj:inspect()

  Player:inspect();
  obj:move_to(player)
  Core.say("You took the " .. obj.title)
end


function Commands.drop(objName)
  print('DROP: ' .. objName)
  Core.say("You tried to drop it, but inventory is not implemented")
end


function Commands.look()
  Core.describe_scene()
end


function Commands.look_at(obj)
  print("You see: " .. obj.name)
end


function Commands.drop_object(obj)
	obj.move_to(Player.location)
end

