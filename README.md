CustomConstraintInjector is a Randomizer 4 connection which allows defining constraints for which items can be placed at which locations, to provide finer control over randomization. It also allows editing priorities of items and locations, to control which items and locations may be used for early progression and which may be used for late progression during randomization.

A constraint pack is a json file written to work with CustomConstraintInjector. To use the pack, place it in the CustomConstraintInjector mod folder (e.g., `Hollow Knight\hollow_knight_Data\Managed\Mods\CustomConstraintInjector`), then start up the game and enable it from the CustomConstraintInjector connections menu.

## Pack Format

The pack json should be a json object with a `Name` property, which must be unique and will be used as the button label in the menu. It may have `Constraints` and `PriorityEdits` properties, which should be json arrays of the respective objects.

A constraint, in json, should have a `Target` property, whose value is a `TargetMatch` object (see below), a `Type` property, whose value is one of the strings noted below in the Constraint Types section, an optional `Params` property whose value is a json array of strings, and an optional `Strict` property whose value is `true` or `false` (defaulting to `false` if omitted).

A priority edit, in json, should have a `Target` property, whose value is a `TargetMatch` object (see below), an `Op` property, whose value is one of the operators noted below in the Priority Operators section, and a decimal `Param` property.

A `TargetMatch` object is defined with a `match` property whose value is one of the following strings (ITEM, ITEM_WILDCARD, ITEM_REGEX, LOCATION, LOCATION_WILDCARD, LOCATION_REGEX), and a `name` property whose value is a string. The ITEM and LOCATION types look for an exact name match. The ITEM_WILDCARD and LOCATION_WILDCARD types allow inserting the `*` character in the name as a wildcard for 0 or more characters, and the `?` character in the name as a wildcard for exactly 1 character. The ITEM_REGEX and LOCATION_REGEX types allow using more complex patterns which are well-documented online.

## Constraint Types

The following constraint types are currently supported. Note that each constraint comes with a target and a list of parameters.
- NOTLOCATION
  - Satisfied when the target item is not placed at a location whose name appears in the parameters list.
- NOTITEM
  - Satisfied when the target location is not placed with an item whose name appears in the parameters list.
- NOTREQUIREDITEM
  - Satisfied when the target location is not placed with a required progression item.
- REQUIREDITEM
  - Satisfied when the target location is placed with a required progression item. Due to the mechanics of randomization, this is unlikely to have an effect unless strict.
- NOTFLEXIBLECOUNT
  - Satisfied when the target item is not placed at a flexible count location. This typically includes geo shops, Grubfather, Seer, and Egg_Shop.
- NOTMAPAREA
  - Satisfied when the target item is not placed in any map area whose name appears in the parameters list.
- NOTTITLEDAREA
  - Satisfied when the target item is not placed in any titled area whose name appears in the parameters list.
- ATMAPAREA
  - Satisfied when the target item is placed in any map area whose name appears in the parameters list.
- ATTITLEDAREA
  - Satisfied when the target item is placed in any titled area whose name appears in the parameters list.

Constraints may be strict or nonstrict, and default to nonstrict. If the randomizer cannot satisfy a strict constraint, its current attempt will fail and it will begin a new attempt. On the other hand, if the randomizer cannot satisfy a nonstrict constraint, it will choose a random legal placement and continue (in particular, it does not weight placements based on partially satisfying constraints). Strict constraints should be used with great care, since they may greatly increase the number of attempts taken by the randomizer. For example, with default settings, the "French Vanilla Skills" pack included in the examples can take hundreds of attempts. Additionally, if the strict constraint is impossible to satisfy, there is no guarantee that randomization will ever succeed. Of note, there are some factors which do not reset between attempts, including start location, cost randomization, and the number of slots at flexible locations, so special attention should be given to how a strict constraint may depend on these.

## Priority Operators

The following priority operators are currently supported. Note that each operator comes with a parameter.
- ADD
  - Increment the target's priority by the parameter
- MUL
  - Multiply the target's priority by the parameter
- MAX
  - Set the target's priority to the maximum of its current value and the parameter (NOT to be confused with bounding the target's priority from above by the parameter)
- MIN
  - Set the target's priority to the minimum of its current value and the parameter (NOT to be confused with bounding the target's priority from below by the parameter)
- POW
  - Set the target's priority to its current value raised to the parameter. If the target is negative, its absolute value is raised to the parameter, then multiplied by -1.

Higher priorities tend to lead to items appearing later in progression, and locations being filled later in randomization. Priorities are initially distributed as uniformly spaced in [0,1] for items and locations.