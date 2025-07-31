-- Core base class system
function create_prototype(base)
    local prototype = base or {}
    prototype.__index = prototype
    function prototype:new(o)
        o = o or {}
        setmetatable(o, self)
        return o
    end
    return prototype
end

-- Game base object prototype
GameObject = create_prototype()
GameObject.Name = "Unnamed"
GameObject.Description = "No description."


Commands = {}

function Commands.inspect(obj)
  print("OBJ: " .. obj.Name)
end


Commands.inspect(GameObject)
