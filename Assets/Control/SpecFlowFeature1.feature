  @smoke @atm
  Feature: Flight Control
  
  Background: 
    Given I'm in the air
      And I have rockets
  
  Scenario Outline: Fly direction planar
    Given I press input towards <direction>
     Then I fly towards <direction>
      And I lean towards <direction>
      And my rockets blast in the opposite <direction>
    Examples: 
      | direction | 
      | 0,1       | 
      | 1,1       | 
      | 1,0       | 
      | 0,0       | 
      | 0.5,1     | 
      | 1,0.5     | 
  
  Scenario: Fly up
    Given I press the jump button
     Then I fly up
      And I extend my body
      And My rockets blast more downwards
  
  Scenario: Fly down
    Given I press the crouch button
     Then I fly down
      And I lean towards the ground
      And my rockets blast more upwards
  
  