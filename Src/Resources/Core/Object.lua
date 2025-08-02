BaseObject = create_prototype()
BaseObject.name = "Unnamed"
BaseObject.description = "No description."
BaseObject.location = nil


function BaseObject:init()
  self.objects_here = {}
end


function BaseObject:move_to(location)
  if self.location then
    self.location:remove_object(self)
  end
  self.location = location
  if location then
    location:include_object(self)
  end  
end


function BaseObject:include_object(obj)
  table.insert(self.objects_here, obj)
end


function BaseObject:remove_object(obj)
  remove_reference_from_list(self.objects_here, obj)
end


-- Inspect any object for debugging.
function BaseObject:inspect()
  print("'" .. self.name .. "' is at '" .. ((self.location and self.location.name) or "<nowhere>") .. "'")
  for i, v in ipairs(self.objects_here) do
    print("  '" .. v.name .. "' is here")
  end
end
