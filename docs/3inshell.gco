M24
M190 S-30.0000
M109 S-30.000000
G20        ;Inch values
G90        ;absolute positioning
M31
M117 Loading 3inshell.gco
G4 S1 ;Pause for button release
M117 3" Shell - Start?
G92 X0 Y0
M227 P2 S0 ;pause, wait for start button
M117 Move Spring Lock
M300 S300 P300
G4 P500
M300 S400 P300
G4 P500
M300 S500 P1000
G4 P1500
M227 P2 S0 ;pause, wait for start button
M75
M117 Wrapping 3" Shell

G1 X0.0Y0.0F900
G1 X7.785Y7.785
G1 X10.377Y8.174
G1 X18.182Y15.979
G1 X20.774Y16.368 
G1 X28.569Y24.163
G1 X31.161Y24.552
G1 X38.976Y32.367
G1 X41.568Y32.756
G1 X49.373Y40.56
G1 X51.965Y40.95
G1 X59.79Y48.775
G1 X62.382Y49.164
G1 X70.197Y56.979
G1 X75.789Y60.368
G1 X83.624Y68.203
G1 X86.216Y68.592
G1 X94.041Y76.417
G1 X96.633Y76.806
G1 X104.478Y84.651
G1 X107.07Y85.04
G1 X114.905Y92.875
G1 X117.497Y93.264
G1 X125.352Y101.119
G1 X127.944Y101.508
G1 X135.789Y109.353
G1 X138.381Y109.742
G1 X146.246Y117.607
G1 X151.838Y120.996
G1 X159.693Y128.851
G1 X162.285Y129.24
G1 X170.16Y137.115
G1 X172.752Y137.504
G1 X180.617Y145.369
G1 X183.209Y145.758
G1 X191.094Y153.643
G1 X193.686Y154.032
G1 X201.561Y161.907
G1 X204.153Y162.296
G1 X212.048Y170.191
G1 X214.64Y170.58
G1 X222.525Y178.465
G1 X228.117Y181.854
G1 X236.022Y189.759
G1 X238.614Y190.148
G1 X246.509Y198.043
G1 X249.101Y198.432
G1 X257.016Y206.347
G1 X259.608Y206.736
G1 X267.513Y214.641
G1 X270.105Y215.03
G1 X278.03Y222.955
G1 X280.622Y223.344
G1 X288.537Y231.259
G1 X291.129Y231.648
G1 X299.064Y239.583
G1 X304.656Y242.972
G1 X312.581Y250.897
G1 X315.173Y251.286
G1 X323.118Y259.231
G1 X325.71Y259.62
G1 X333.645Y267.555
G1 X336.237Y267.944
G1 X344.192Y275.899
G1 X346.784Y276.288
G1 X354.729Y284.233
G1 X357.321Y284.622
G1 X365.286Y292.587
G1 X367.878Y292.976
G1 X375.833Y300.931
G1 X381.425Y304.32
G1 X389.4Y312.295
G1 X391.992Y312.684
G1 X399.957Y320.649
G1 X402.549Y321.038
G1 X410.534Y329.023
G1 X413.126Y329.412
G1 X421.101Y337.387
G1 X423.693Y337.776
G1 X431.688Y345.771
G1 X434.28Y346.16
G1 X442.265Y354.145
G1 X444.857Y354.534
G1 X452.862Y362.539
G1 X458.454Y365.928
G1 X466.449Y373.923
G1 X469.041Y374.312
G1 X477.056Y382.327
G1 X479.648Y382.716
G1 X487.653Y390.721
G1 X490.245Y391.11
G1 X498.27Y399.135
G1 X500.862Y399.524
G1 X508.877Y407.539
G1 X511.469Y407.928
G1 X519.504Y415.963
G1 X522.096Y416.352
G1 X530.121Y424.377
G1 X535.713Y427.766
G1 X543.758Y435.811
G1 X546.35Y436.2
G1 X554.385Y444.235
G1 X556.977Y444.624
G1 X565.032Y452.679
G1 X567.624Y453.068
G1 X575.669Y461.113
G1 X578.261Y461.502
G1 X586.326Y469.567
G1 X588.918Y469.956
G1 X596.973Y478.011
G1 X599.565Y478.4
G1 X607.64Y486.475
G1 X613.232Y489.864
G1 X621.297Y497.929
G1 X623.889Y498.318
G1 X631.974Y506.403
G1 X634.566Y506.792
G1 X642.641Y514.867
G1 X645.233Y515.256
G1 X653.328Y523.351
G1 X655.92Y523.74
G1 X664.005Y531.825
G1 X666.597Y532.214
G1 X674.702Y540.319
G1 X677.294Y540.708
G1 X685.389Y548.803
G1 X690.981Y552.192
G1 X699.096Y560.307
G1 X701.688Y560.696
G1 X709.793Y568.801
G1 X712.385Y569.19
G1 X720.51Y577.315
G1 X723.102Y577.704
G1 X731.217Y585.819
G1 X733.809Y586.208
G1 X741.944Y594.343
G1 X744.536Y594.732
G1 X752.661Y602.857
G1 X755.253Y603.246
G1 X763.398Y611.391
G1 X768.99Y614.78
G1 X777.125Y622.915
G1 X779.717Y623.304

M400
G92 X0 Y0
M300 S300 P300
G4 P500
M300 S400 P300
G4 P500
M300 S500 P1000
G4 P1500
M300 S500 P1500
M117 Paused, Tear Tape
M227 P2 S0
M76
M117 Burnishing 
M75

G1 X0.0Y0.0F900
G1 X7.785Y7.785
G1 X10.377Y8.174
G1 X18.182Y15.979
G1 X20.774Y16.368
G1 X28.569Y24.163
G1 X31.161Y24.552F1000
G1 X38.976Y32.367
G1 X41.568Y32.756
G1 X49.373Y40.561
G1 X51.965Y40.95
G1 X59.79Y48.775
G1 X62.382Y49.164f1200
G1 X70.197Y56.979
G1 X75.789Y60.368
G1 X83.624Y68.203
G1 X86.216Y68.592F2400
G1 X94.041Y76.417
G1 X96.633Y76.806
G1 X104.478Y84.651
G1 X107.07Y85.04
G1 X114.905Y92.875
G1 X117.497Y93.264
G1 X125.352Y101.119
G1 X127.944Y101.508
G1 X135.789Y109.353
G1 X138.381Y109.742
G1 X146.246Y117.607
G1 X151.838Y120.996
G1 X159.693Y128.851
G1 X162.285Y129.24
G1 X170.16Y137.115
G1 X172.752Y137.504
G1 X180.617Y145.369
G1 X183.209Y145.758
G1 X191.094Y153.643
G1 X193.686Y154.032
G1 X201.561Y161.907
G1 X204.153Y162.296
G1 X212.048Y170.191
G1 X214.64Y170.58
G1 X222.525Y178.465
G1 X228.117Y181.854
G1 X236.022Y189.759
G1 X238.614Y190.148
G1 X246.509Y198.043
G1 X249.101Y198.432
G1 X257.016Y206.347
G1 X259.608Y206.736
G1 X267.513Y214.641
G1 X270.105Y215.03
G1 X278.03Y222.955
G1 X280.622Y223.344
G1 X288.537Y231.259
G1 X291.129Y231.648
G1 X299.064Y239.583
G1 X304.656Y242.972
G1 X312.581Y250.897
G1 X315.173Y251.286
G1 X323.118Y259.231
G1 X325.71Y259.62
G1 X333.645Y267.555
G1 X336.237Y267.944
G1 X344.192Y275.899
G1 X346.784Y276.288
G1 X354.729Y284.233
G1 X357.321Y284.622
G1 X365.286Y292.587
G1 X367.878Y292.976
G1 X375.833Y300.931
G1 X381.425Y304.32
G1 X389.4Y312.295
G1 X391.992Y312.684
G1 X399.957Y320.649
G1 X402.549Y321.038
G1 X410.534Y329.023
G1 X413.126Y329.412
G1 X421.101Y337.387
G1 X423.693Y337.776
G1 X431.688Y345.771
G1 X434.28Y346.16
G1 X442.265Y354.145
G1 X444.857Y354.534
G1 X452.862Y362.539
G1 X458.454Y365.928
G1 X466.449Y373.923
G1 X469.041Y374.312
G1 X477.056Y382.327
G1 X479.648Y382.716
G1 X487.653Y390.721
G1 X490.245Y391.11
G1 X498.27Y399.135
G1 X500.862Y399.524
G1 X508.877Y407.539
G1 X511.469Y407.928
G1 X519.504Y415.963
G1 X522.096Y416.352
G1 X530.121Y424.377
G1 X532.713Y424.766
G1 X540.758Y432.811
G1 X543.35Y433.2
G1 X551.385Y441.235
G1 X553.977Y441.624
G1 X562.032Y449.679
G1 X564.624Y450.068
G1 X572.669Y458.113
G1 X575.261Y458.502
G1 X583.326Y466.567
G1 X585.918Y466.956
G1 X593.973Y475.011
G1 X596.565Y475.4
G1 X604.64Y483.475
G1 X607.232Y483.864
G1 X615.297Y491.929
G1 X617.889Y492.318
G1 X625.974Y500.403
G1 X628.566Y500.792
G1 X636.641Y508.867
G1 X639.233Y509.256
G1 X647.328Y517.351
G1 X649.92Y517.74
G1 X658.005Y525.825
G1 X660.597Y526.214
G1 X668.702Y534.319
G1 X671.294Y534.708

M400
M300 S300 P300
G4 P500
M300 S400 P300
G4 P500
M300 S500 P1000
G4 P1500
M117 Complete- Rewind?
M227 P2 S0
M23 /3inshell.gco
M24
