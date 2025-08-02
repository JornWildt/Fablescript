
function Commands.take(name)
  print('TAKE: ' .. name)
  Player:inspect();

  if Player.location then
    local obj = Player:find_contained_object_by_name(name)
    if obj then
      obj:inspect()
      Core.say("You already have the " .. obj.title)
    else
      obj = Player.location:find_contained_object_by_name(name)
      if obj then
        obj:move_to(Player)
        Core.say("You took the " .. obj.title)
      else
        Core.say("It is not here")
      end
    end
  else
    Core.say("Nothing here")
  end
end


function Commands.drop(name)
  print('DROP: ' .. name)
  Player:inspect();

  if Player.location then
    local obj = Player:find_contained_object_by_name(name)
    if obj then
      obj:inspect()
      obj:move_to(Player.location)
      Core.say('You dropped the ' .. obj.title)
    else
      Core.say("You do not have the " .. obj.title)
    end
  else
    Core.say("Nothing here")
  end
end


function Commands.inventory()
  print('INVENTORY')
  Player:inspect();

  if next(Player.objects_here) == nil then
    Core.say("You have no objects.")
  else
    for i, v in ipairs(Player.objects_here) do
      Core.say("- " .. v.title)
    end
  end
end


function Commands.inspect(name)
  local obj = nil
  if Player.location then
    obj = Player.location:find_contained_object_by_name(name)
  end

  if not obj then
    obj = Player:find_contained_object_by_name(name)
  end

  if obj then
    if obj.description then
      Core.say(obj.description)
    else
      Core.say("Nothing special about " .. obj.title)
    end
  else
    Core.say("Not here")
  end
end

