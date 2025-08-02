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
