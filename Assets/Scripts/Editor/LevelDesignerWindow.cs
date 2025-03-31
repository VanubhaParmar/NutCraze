using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Tag.NutSort;
using System;
using System.Linq;

// Define this enum if it doesn't exist elsewhere
public class NutSortLevelDesignerWindow_V3 : EditorWindow // Renamed
{
    // --- Editor State ---
    private enum EditorViewMode { LevelSelection, StageOverview, ScrewEdit }
    private EditorViewMode currentViewMode = EditorViewMode.LevelSelection;
    private int selectedLevelStageIndex = -1;
    private int selectedScrewDataIndex = -1; // Index within the current LevelStage's screwDatas

    // --- Level Data State ---
    private LevelData currentLevelData;
    private int currentLevelNumber = 1;
    private LevelType currentLevelType = LevelType.NORMAL_LEVEL;
    private ABTestType currentABTestType = ABTestType.Default;
    private bool isDataLoaded = false;
    private bool isDirty = false;
    private string statusMessage = "Select AB Test, Level Type, enter Level Number and Load or Create.";
    private Vector2 scrollPosMain; // Scroll for the main area
    private Vector2 scrollPosScrewStages; // Scroll specifically for the internal stages list

    // --- Visual Configuration (NEEDS YOUR ACTUAL DATA) ---
    private Dictionary<int, Color> editorColorMap = new Dictionary<int, Color>()
    { { 0, Color.red }, { 1, Color.blue }, { 2, Color.green }, { 3, Color.yellow }, { 4, new Color(0.8f, 0.4f, 0) }, { 5, Color.magenta } };
    private Color emptySlotColor = Color.grey * 0.7f;
    private Color defaultColor = Color.white;
    private List<int> availableNutColorIds = new List<int>() { 0, 1, 2, 3, 4, 5 };
    private const int EMPTY_NUT_ID = -1;
    // Placeholder for Screw Type mapping (ID -> Name) - YOU NEED TO PROVIDE THIS
    private Dictionary<int, string> screwTypeMap = new Dictionary<int, string>()
    { {0, "Simple Screw"}, {1, "Booster Screw"}, {2, "Locked Screw"} };
    // Placeholder for Nut Type mapping (ID -> Name) - YOU NEED TO PROVIDE THIS
    private Dictionary<int, string> nutTypeMap = new Dictionary<int, string>()
    { {0, "Normal Nut"}, {1, "Blocker Nut"}, {2, "Color Nut"} };


    // --- Configuration ---
    private bool useCompression = LevelDataFactory.UseCompression;
    private bool useEncryption = LevelDataFactory.UseEncryption;
    private int levelsPerChunk = LevelDataFactory.LevelsPerChunk;

    // --- Clipboard (Basic Placeholder) ---
    private static ScrewData copiedScrewData = null;
    private static ScrewStage copiedScrewStage = null;


    // --- Window Setup ---
    [MenuItem("Tools/Nut Sort/Level Designer (V3 - Image Layout)")]
    public static void ShowWindow()
    {
        GetWindow<NutSortLevelDesignerWindow_V3>("Nut Sort Designer V3");
    }

    // --- GUI ---
    private void OnGUI()
    {
        // Always show level selection/load at the top? Or only in LevelSelection mode?
        // Let's keep it always visible for quick loading.
        DrawTopLevelControls();
        EditorGUILayout.Space();

        scrollPosMain = EditorGUILayout.BeginScrollView(scrollPosMain);

        // Switch view based on mode
        switch (currentViewMode)
        {
            case EditorViewMode.LevelSelection:
                // Mostly handled by DrawTopLevelControls, show info message
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
                break;

            case EditorViewMode.StageOverview:
                if (isDataLoaded && currentLevelData != null && selectedLevelStageIndex != -1)
                {
                    DrawStageOverviewGUI();
                }
                else
                {
                    EditorGUILayout.HelpBox("Load a level and select a stage.", MessageType.Warning);
                    currentViewMode = EditorViewMode.LevelSelection; // Go back if state is invalid
                }
                break;

            case EditorViewMode.ScrewEdit:
                if (isDataLoaded && currentLevelData != null && selectedLevelStageIndex != -1 && selectedScrewDataIndex != -1)
                {
                    DrawScrewEditGUI();
                }
                else
                {
                    EditorGUILayout.HelpBox("Load a level, select a stage, and select a screw to edit.", MessageType.Warning);
                    currentViewMode = EditorViewMode.StageOverview; // Go back if state is invalid
                }
                break;
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Status: " + statusMessage, EditorStyles.wordWrappedMiniLabel);

        // Save Button - always visible at bottom?
        if (isDataLoaded)
        {
            GUI.backgroundColor = isDirty ? Color.yellow : Color.white;
            if (GUILayout.Button("Save Level Data")) SaveLevel();
            GUI.backgroundColor = Color.white;
        }
    }

    // --- Top Level Controls ---
    void DrawTopLevelControls()
    {
        EditorGUILayout.LabelField("Level Selection & Management", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        currentABTestType = (ABTestType)EditorGUILayout.EnumPopup("AB Test Type", currentABTestType);
        currentLevelType = (LevelType)EditorGUILayout.EnumPopup("Level Type", currentLevelType);
        currentLevelNumber = EditorGUILayout.IntField("Level Number", currentLevelNumber);
        if (EditorGUI.EndChangeCheck())
        {
            ResetFullEditorState(); // Full reset if core selection changes
            statusMessage = "Selection changed. Load or Create.";
        }
        currentLevelNumber = Mathf.Max(1, currentLevelNumber);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Level")) LoadLevel();
        if (GUILayout.Button("Create New/Overwrite Level"))
        {
            if (!isDirty || EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes. Are you sure?", "Yes", "No"))
                CreateOrPrepareLevel();
        }
        EditorGUILayout.EndHorizontal();

        if (isDataLoaded && currentLevelData != null)
        {
            EditorGUILayout.LabelField($"Editing Level: {currentLevelData.level} ({currentLevelData.levelType})", EditorStyles.miniLabel);
        }

    }


    // --- View: Stage Overview ---
    void DrawStageOverviewGUI()
    {
        LevelStage currentStage = GetCurrentLevelStage();
        if (currentStage == null) return;

        // --- Stage Navigation ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(30))) ChangeSelectedLevelStage(-1);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"Level Stage: {selectedLevelStageIndex + 1} / {currentLevelData.stages.Length}", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(">", GUILayout.Width(30))) ChangeSelectedLevelStage(1);
        EditorGUILayout.EndHorizontal();

        // Add Level Stage Button (if needed, or manage outside?)
        // if (GUILayout.Button("Add Level Stage (+)")) { AddLevelStage(); GUIUtility.ExitGUI(); return; }
        // Remove Level Stage Button
        // if (GUILayout.Button("Remove Current Level Stage (-)")) { RemoveLevelStage(selectedLevelStageIndex); GUIUtility.ExitGUI(); return; }


        // --- Arrangement ---
        EditorGUI.BeginChangeCheck();
        currentStage.arrangementId = EditorGUILayout.IntField("Stage Arrangement ID", currentStage.arrangementId);
        if (EditorGUI.EndChangeCheck()) isDirty = true;
        // Display mapping if available: EditorGUILayout.LabelField($"Arrangement: {GetArrangementName(currentStage.arrangementId)}");
        EditorGUILayout.Space();

        // --- Screw Grid ---
        EditorGUILayout.LabelField("Screw Definitions", EditorStyles.boldLabel);
        if (currentStage.screwDatas == null) currentStage.screwDatas = new ScrewData[0];

        int screwsPerRow = 4; // Adjust for desired grid width
        int rowCount = Mathf.CeilToInt((float)currentStage.screwDatas.Length / screwsPerRow);

        for (int r = 0; r < rowCount; r++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Center rows potentially
            for (int i = 0; i < screwsPerRow; i++)
            {
                int screwIndex = r * screwsPerRow + i;
                if (screwIndex < currentStage.screwDatas.Length)
                {
                    DrawScrewOverviewBox(currentStage.screwDatas[screwIndex], screwIndex);
                }
                else
                {
                    GUILayout.Space(160); // Placeholder for empty grid slots, adjust width as needed
                }
                if (i < screwsPerRow - 1) GUILayout.Space(10); // Space between items in a row
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10); // Space between rows
        }


        if (GUILayout.Button("Add Screw Definition (+)"))
        {
            AddScrewData(selectedLevelStageIndex);
            GUIUtility.ExitGUI();
        }
    }

    void DrawScrewOverviewBox(ScrewData screwData, int screwIndex)
    {
        if (screwData == null) return; // Should be init

        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(150)); // Adjust width

        // Header
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{screwIndex + 1}. Screw", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        // Basic Copy/Paste Placeholders
        if (GUILayout.Button("C", EditorStyles.miniButton, GUILayout.Width(20))) { CopyScrewData(screwData); }
        if (GUILayout.Button("P", EditorStyles.miniButton, GUILayout.Width(20))) { PasteScrewData(selectedLevelStageIndex, screwIndex); }
        EditorGUILayout.EndHorizontal();

        // Info
        string screwTypeName = screwTypeMap.TryGetValue(screwData.screwType, out string name) ? name : $"Type ID: {screwData.screwType}";
        EditorGUILayout.LabelField(screwTypeName, EditorStyles.miniLabel); // Display Screw Type Name

        EditorGUILayout.LabelField($"Nut Capacity: {screwData.size}", EditorStyles.miniLabel);
        int internalStageCount = screwData.screwStages?.Length ?? 0;
        EditorGUILayout.LabelField($"Total Stages: {internalStageCount}", EditorStyles.miniLabel);

        // Edit Button
        if (GUILayout.Button("Edit"))
        {
            selectedScrewDataIndex = screwIndex;
            currentViewMode = EditorViewMode.ScrewEdit;
            Repaint(); // Force redraw in new mode
        }

        EditorGUILayout.EndVertical();
    }


    // --- View: Screw Edit ---
    void DrawScrewEditGUI()
    {
        LevelStage currentStage = GetCurrentLevelStage();
        ScrewData currentScrew = GetCurrentScrewData();
        if (currentStage == null || currentScrew == null) return;

        // --- Header & Navigation ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Back", GUILayout.Width(50)))
        {
            currentViewMode = EditorViewMode.StageOverview;
            selectedScrewDataIndex = -1; // Deselect screw
            Repaint();
            return; // Exit GUI for this frame
        }
        GUILayout.Space(10);
        if (GUILayout.Button("<", GUILayout.Width(30))) ChangeSelectedScrewData(-1);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"{selectedScrewDataIndex + 1}. Screw (ID: {currentScrew.id})", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(">", GUILayout.Width(30))) ChangeSelectedScrewData(1);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // --- Main Edit Area (Left: Stages, Right: Properties) ---
        EditorGUILayout.BeginHorizontal();

        // --- Left Panel: Internal Stages List ---
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f)); // Allocate ~60% width
        EditorGUILayout.LabelField("Internal Screw Stages", EditorStyles.boldLabel);
        scrollPosScrewStages = EditorGUILayout.BeginScrollView(scrollPosScrewStages, EditorStyles.helpBox); // Scroll view for stages

        if (currentScrew.screwStages == null) currentScrew.screwStages = new ScrewStage[0];
        for (int i = 0; i < currentScrew.screwStages.Length; i++)
        {
            DrawInternalScrewStageEdit(currentScrew.screwStages[i], currentScrew, i);
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndScrollView(); // End stages scroll view

        if (GUILayout.Button("Add Internal Screw Stage (+)"))
        {
            AddInternalScrewStage(currentScrew);
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndVertical(); // End Left Panel

        // --- Right Panel: Screw Properties ---
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true)); // Expand to fill remaining width
        EditorGUILayout.LabelField("Screw Properties", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck(); // Track property changes

        // Screw Type Dropdown (Example)
        int selectedScrewTypeIndex = screwTypeMap.Keys.ToList().IndexOf(currentScrew.screwType);
        if (selectedScrewTypeIndex < 0) selectedScrewTypeIndex = 0; // Default to first if not found
        int newScrewTypeIndex = EditorGUILayout.Popup("Screw Type", selectedScrewTypeIndex, screwTypeMap.Values.ToArray());
        int newScrewType = screwTypeMap.Keys.ElementAt(newScrewTypeIndex);
        if (newScrewType != currentScrew.screwType)
        {
            currentScrew.screwType = newScrewType;
        }


        // Nut Capacity
        int oldSize = currentScrew.size;
        currentScrew.size = EditorGUILayout.IntField("Nut Capacity", currentScrew.size);
        currentScrew.size = Mathf.Max(1, currentScrew.size);

        // Other ScrewData properties if any (ID is usually set on creation/load)
        using (new EditorGUI.DisabledScope(true)) { EditorGUILayout.IntField("Screw ID", currentScrew.id); }

        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
            if (currentScrew.size != oldSize)
            {
                // IMPORTANT: Resize NutData arrays in all internal stages if capacity changed
                ResizeNutDataInAllInternalStages(currentScrew);
            }
        }
        EditorGUILayout.EndVertical(); // End Right Panel

        EditorGUILayout.EndHorizontal(); // End Main Edit Area
    }


    void DrawInternalScrewStageEdit(ScrewStage internalStage, ScrewData parentScrew, int stageIndex)
    {
        if (internalStage == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.textArea); // Box for the internal stage

        // Header
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Screw Stage {stageIndex + 1}", EditorStyles.miniBoldLabel);
        GUILayout.FlexibleSpace();
        // Placeholder Copy/Paste
        if (GUILayout.Button("C", EditorStyles.miniButton, GUILayout.Width(20))) { CopyScrewStage(internalStage); }
        if (GUILayout.Button("P", EditorStyles.miniButton, GUILayout.Width(20))) { PasteScrewStage(parentScrew, stageIndex); }
        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20))) { RemoveInternalScrewStage(parentScrew, stageIndex); GUIUtility.ExitGUI(); return; }
        EditorGUILayout.EndHorizontal();

        // --- Nut Stack Representation ---
        ResizeNutDataArrayIfNeeded(internalStage, parentScrew.size); // Ensure array matches capacity
        for (int i = 0; i < parentScrew.size; i++)
        {
            DrawNutSlotEditRow(internalStage, i);
        }
        // --- End Nut Stack ---

        // Other ScrewStage properties (optional, could be behind a foldout)
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        internalStage.isStorage = EditorGUILayout.ToggleLeft("Store", internalStage.isStorage, GUILayout.Width(60));
        internalStage.isRefresh = EditorGUILayout.ToggleLeft("Refresh", internalStage.isRefresh, GUILayout.Width(70));
        internalStage.isGenerator = EditorGUILayout.ToggleLeft("Gen", internalStage.isGenerator, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck()) isDirty = true;


        EditorGUILayout.EndVertical(); // End internal stage box
    }

    void DrawNutSlotEditRow(ScrewStage internalStage, int slotIndex)
    {
        // NutData array should be correctly sized by now
        NutData nut = (internalStage.nutDatas != null && slotIndex < internalStage.nutDatas.Length) ? internalStage.nutDatas[slotIndex] : null;
        int currentNutColorId = (nut == null) ? EMPTY_NUT_ID : nut.nutColorTypeId;
        int currentNutTypeId = (nut == null) ? -1 : nut.nutType; // Assuming -1 for no nut type


        EditorGUILayout.BeginHorizontal();

        // Slot Index Display
        EditorGUILayout.LabelField($"{slotIndex + 1}.", GUILayout.Width(25));

        // Nut Type Selector (Example Dropdown)
        int selectedNutTypeIndex = nutTypeMap.Keys.ToList().IndexOf(currentNutTypeId);
        // Handle case where there's no nut (-1) or type isn't in map
        bool hasNut = nut != null;
        if (!hasNut) selectedNutTypeIndex = -1; // Or a specific "None" index if added to map

        // Need a way to represent "No Nut" in the dropdown
        List<string> nutTypeDisplayNames = new List<string> { "None" };
        nutTypeDisplayNames.AddRange(nutTypeMap.Values);
        int currentDisplayIndex = hasNut ? selectedNutTypeIndex + 1 : 0; // Shift index because of "None"

        int newDisplayIndex = EditorGUILayout.Popup(currentDisplayIndex, nutTypeDisplayNames.ToArray(), GUILayout.MinWidth(80));

        if (newDisplayIndex != currentDisplayIndex)
        {
            isDirty = true;
            if (newDisplayIndex == 0) // Selected "None"
            {
                internalStage.nutDatas[slotIndex] = null; // Remove nut
            }
            else
            {
                int newNutTypeMapIndex = newDisplayIndex - 1;
                if (newNutTypeMapIndex >= 0 && newNutTypeMapIndex < nutTypeMap.Count)
                {
                    int newNutTypeId = nutTypeMap.Keys.ElementAt(newNutTypeMapIndex);
                    if (nut == null) // Was empty, create new nut
                    {
                        internalStage.nutDatas[slotIndex] = new NutData { nutType = newNutTypeId, nutColorTypeId = EMPTY_NUT_ID }; // Default color?
                    }
                    else
                    {
                        nut.nutType = newNutTypeId; // Update existing nut type
                    }
                }
            }
            // Refresh currentNutColorId after potential change
            nut = internalStage.nutDatas[slotIndex];
            currentNutColorId = (nut == null) ? EMPTY_NUT_ID : nut.nutColorTypeId;
        }


        // Nut Color Button (only if a nut exists)
        using (new EditorGUI.DisabledScope(nut == null))
        {
            Color buttonColor = GetColorFromNutId(currentNutColorId);
            string buttonText = (currentNutColorId == EMPTY_NUT_ID) ? "Clr?" : $"{currentNutColorId}"; // Text for color button
            GUI.backgroundColor = buttonColor;
            if (GUILayout.Button(buttonText, GUILayout.Width(50)))
            {
                if (nut != null) // Should always be true if not disabled, but check
                {
                    CycleNutColorInSlot(internalStage, slotIndex);
                    isDirty = true;
                }
            }
            GUI.backgroundColor = Color.white;
        }

        // Placeholder for other fields like "1. Block" if needed
        // EditorGUILayout.TextField("...", GUILayout.Width(50));


        // Remove Nut Button (-)
        if (GUILayout.Button("-", GUILayout.Width(25)))
        {
            if (internalStage.nutDatas != null && slotIndex < internalStage.nutDatas.Length)
            {
                internalStage.nutDatas[slotIndex] = null; // Set slot to empty
                isDirty = true;
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // --- Data Modification Helpers --- (Includes Add/Remove/Cycle/Resize)

    void CycleNutColorInSlot(ScrewStage internalStage, int slotIndex)
    {
        if (internalStage.nutDatas == null || slotIndex >= internalStage.nutDatas.Length || internalStage.nutDatas[slotIndex] == null) return; // No nut to cycle

        NutData currentNut = internalStage.nutDatas[slotIndex];
        int currentNutColorId = currentNut.nutColorTypeId;

        int currentListIndex = availableNutColorIds.IndexOf(currentNutColorId);
        int nextListIndex = currentListIndex + 1;

        int nextNutColorId;
        // Cycle through available colors, potentially including -1 if desired, or just wrap colors
        if (nextListIndex >= availableNutColorIds.Count)
        {
            // Option 1: Wrap back to first color
            // nextNutColorId = availableNutColorIds.Count > 0 ? availableNutColorIds[0] : EMPTY_NUT_ID;
            // Option 2: Go to EMPTY_NUT_ID after last color
            nextNutColorId = EMPTY_NUT_ID;
        }
        else
        {
            nextNutColorId = availableNutColorIds[nextListIndex];
        }
        currentNut.nutColorTypeId = nextNutColorId;
    }

    void ResizeNutDataArrayIfNeeded(ScrewStage internalScrewStage, int targetSize)
    {
        // ... (same implementation as V2) ...
        int currentLength = internalScrewStage.nutDatas?.Length ?? 0;
        if (currentLength == targetSize) return;
        NutData[] newNutArray = new NutData[targetSize];
        for (int i = 0; i < targetSize; i++)
        {
            newNutArray[i] = (i < currentLength && internalScrewStage.nutDatas[i] != null) ? internalScrewStage.nutDatas[i] : null;
        }
        internalScrewStage.nutDatas = newNutArray;
        // Don't necessarily set isDirty here, as it might be called during validation or load
    }
    void ResizeNutDataInAllInternalStages(ScrewData screwData)
    {
        // ... (same implementation as V2) ...
        if (screwData?.screwStages == null) return;
        foreach (var stage in screwData.screwStages)
        {
            if (stage != null) ResizeNutDataArrayIfNeeded(stage, screwData.size);
        }
    }

    // --- Add/Remove ---
    void AddLevelStage()
    { /* ... (same as V2) ... */
        List<LevelStage> stages = currentLevelData.stages?.ToList() ?? new List<LevelStage>();
        stages.Add(new LevelStage() { screwDatas = new ScrewData[0] });
        currentLevelData.stages = stages.ToArray();
        isDirty = true;
        selectedLevelStageIndex = currentLevelData.stages.Length - 1; // Select the new stage
        currentViewMode = EditorViewMode.StageOverview; // Ensure we are in the right view
    }
    void RemoveLevelStage(int index)
    { /* ... (same as V2, needs index adjustment) ... */
        if (currentLevelData.stages == null || index < 0 || index >= currentLevelData.stages.Length) return;
        List<LevelStage> stages = currentLevelData.stages.ToList();
        stages.RemoveAt(index);
        currentLevelData.stages = stages.ToArray();
        isDirty = true;
        // Adjust selected index and view
        if (selectedLevelStageIndex >= index) selectedLevelStageIndex--;
        if (selectedLevelStageIndex < 0 && currentLevelData.stages.Length > 0) selectedLevelStageIndex = 0;
        if (currentLevelData.stages.Length == 0)
        {
            selectedLevelStageIndex = -1;
            currentViewMode = EditorViewMode.LevelSelection;
        }
        else
        {
            currentViewMode = EditorViewMode.StageOverview; // Stay in overview if stages remain
        }
    }
    void AddScrewData(int levelStageIndex)
    { /* ... (same as V2) ... */
        LevelStage stage = GetLevelStage(levelStageIndex);
        if (stage == null) return;
        List<ScrewData> screws = stage.screwDatas?.ToList() ?? new List<ScrewData>();
        int nextId = FindNextAvailableScrewIDGlobally();
        screws.Add(new ScrewData { id = nextId, size = 4, screwStages = new ScrewStage[0], screwType = 0 }); // Default type 0
        stage.screwDatas = screws.ToArray();
        isDirty = true;
    }
    // RemoveScrewData (used by delete button in overview - assumes correct index)
    void RemoveScrewData(int levelStageIndex, int screwDataIndex)
    { /* ... (same as V2) ... */
        LevelStage stage = GetLevelStage(levelStageIndex);
        if (stage == null || stage.screwDatas == null || screwDataIndex < 0 || screwDataIndex >= stage.screwDatas.Length) return;
        List<ScrewData> screws = stage.screwDatas.ToList();
        screws.RemoveAt(screwDataIndex);
        stage.screwDatas = screws.ToArray();
        isDirty = true;
        // If the removed screw was the one being edited, go back to overview
        if (currentViewMode == EditorViewMode.ScrewEdit && selectedLevelStageIndex == levelStageIndex && selectedScrewDataIndex == screwDataIndex)
        {
            currentViewMode = EditorViewMode.StageOverview;
            selectedScrewDataIndex = -1;
        }
        else if (selectedScrewDataIndex >= screwDataIndex)
        {
            // Adjust selected index if it came after the removed one (though usually we leave edit mode)
            // selectedScrewDataIndex--; // Be careful with this if navigation relies on index stability during removal
        }
    }

    void AddInternalScrewStage(ScrewData screwData)
    { /* ... (same as V2) ... */
        if (screwData == null) return;
        List<ScrewStage> stages = screwData.screwStages?.ToList() ?? new List<ScrewStage>();
        ScrewStage newInternalStage = new ScrewStage();
        ResizeNutDataArrayIfNeeded(newInternalStage, screwData.size);
        stages.Add(newInternalStage);
        screwData.screwStages = stages.ToArray();
        isDirty = true;
    }
    void RemoveInternalScrewStage(ScrewData screwData, int internalStageIndex)
    { /* ... (same as V2) ... */
        if (screwData == null || screwData.screwStages == null || internalStageIndex < 0 || internalStageIndex >= screwData.screwStages.Length) return;
        List<ScrewStage> stages = screwData.screwStages.ToList();
        stages.RemoveAt(internalStageIndex);
        screwData.screwStages = stages.ToArray();
        isDirty = true;
    }

    // --- Navigation Helpers ---
    void ChangeSelectedLevelStage(int direction)
    {
        if (currentLevelData?.stages == null || currentLevelData.stages.Length == 0) return;
        selectedLevelStageIndex += direction;
        selectedLevelStageIndex = Mathf.Clamp(selectedLevelStageIndex, 0, currentLevelData.stages.Length - 1);
        selectedScrewDataIndex = -1; // Reset screw selection when changing stage
        currentViewMode = EditorViewMode.StageOverview; // Ensure correct view
        Repaint();
    }
    void ChangeSelectedScrewData(int direction)
    {
        LevelStage stage = GetCurrentLevelStage();
        if (stage?.screwDatas == null || stage.screwDatas.Length == 0) return;
        selectedScrewDataIndex += direction;
        selectedScrewDataIndex = Mathf.Clamp(selectedScrewDataIndex, 0, stage.screwDatas.Length - 1);
        // currentViewMode should already be ScrewEdit
        Repaint();
    }

    // --- State Getters ---
    LevelStage GetCurrentLevelStage() { return GetLevelStage(selectedLevelStageIndex); }
    LevelStage GetLevelStage(int index)
    {
        if (currentLevelData?.stages != null && index >= 0 && index < currentLevelData.stages.Length)
        {
            return currentLevelData.stages[index];
        }
        return null;
    }
    ScrewData GetCurrentScrewData()
    {
        LevelStage stage = GetCurrentLevelStage();
        if (stage?.screwDatas != null && selectedScrewDataIndex >= 0 && selectedScrewDataIndex < stage.screwDatas.Length)
        {
            return stage.screwDatas[selectedScrewDataIndex];
        }
        return null;
    }


    // --- Utility, Init, Validation, Load/Save ---
    private Color GetColorFromNutId(int nutId)
    { /* ... (same as V2) ... */
        if (nutId == EMPTY_NUT_ID) return emptySlotColor;
        if (editorColorMap.TryGetValue(nutId, out Color color)) return color;
        return defaultColor;
    }
    private int FindNextAvailableScrewIDGlobally()
    { /* ... (same as V2) ... */
        int maxId = -1;
        if (currentLevelData?.stages != null)
        {
            foreach (var levelStage in currentLevelData.stages)
            {
                if (levelStage?.screwDatas != null)
                {
                    foreach (var screwData in levelStage.screwDatas)
                    {
                        if (screwData != null && screwData.id > maxId) maxId = screwData.id;
                    }
                }
            }
        }
        return maxId + 1;
    }

    void ResetFullEditorState()
    {
        currentLevelData = null;
        isDataLoaded = false;
        isDirty = false;
        selectedLevelStageIndex = -1;
        selectedScrewDataIndex = -1;
        currentViewMode = EditorViewMode.LevelSelection;
        // statusMessage set by caller
    }
    void LoadLevel()
    { /* ... (same as V2, calls ResetFullEditorState) ... */
        ResetFullEditorState();
        statusMessage = $"Loading Level {currentLevelNumber} ({currentLevelType}/{currentABTestType})...";
        Repaint();
        try
        {
            LevelDataFactory.UseCompression = useCompression; LevelDataFactory.UseEncryption = useEncryption; LevelDataFactory.LevelsPerChunk = levelsPerChunk;
            LevelData loadedData = LevelDataFactory.GetLevelData(currentABTestType, currentLevelType, currentLevelNumber);
            if (loadedData != null)
            {
                currentLevelData = loadedData;
                InitializeNullArraysV2(currentLevelData); // Use V2 init logic
                isDataLoaded = true;
                isDirty = false;
                statusMessage = $"Level {currentLevelNumber} loaded successfully.";
                // Automatically select first stage if available
                selectedLevelStageIndex = (currentLevelData.stages != null && currentLevelData.stages.Length > 0) ? 0 : -1;
                if (selectedLevelStageIndex != -1) currentViewMode = EditorViewMode.StageOverview;
                else currentViewMode = EditorViewMode.LevelSelection; // Stay here if no stages
            }
            else
            {
                statusMessage = $"Level {currentLevelNumber} ({currentLevelType}/{currentABTestType}) not found. You can 'Create' it.";
                EditorGUILayout.HelpBox(statusMessage, MessageType.Warning);
                currentViewMode = EditorViewMode.LevelSelection;
            }
        }
        catch (System.Exception ex)
        {
            ResetFullEditorState(); statusMessage = $"Error loading level: {ex.Message}";
            Debug.LogError($"Error loading level {currentLevelNumber}: {ex.ToString()}");
            EditorGUILayout.HelpBox(statusMessage, MessageType.Error);
        }
    }
    void CreateOrPrepareLevel()
    { /* ... (same as V2, calls ResetFullEditorState) ... */
        ResetFullEditorState();
        statusMessage = $"Preparing Level {currentLevelNumber} ({currentLevelType}/{currentABTestType})...";
        currentLevelData = new LevelData { level = currentLevelNumber, levelType = currentLevelType, stages = new LevelStage[0] };
        InitializeNullArraysV2(currentLevelData);
        isDataLoaded = true; isDirty = true;
        statusMessage = $"New level data created. Add Stages and Screws.";
        selectedLevelStageIndex = -1; // No stages yet
        currentViewMode = EditorViewMode.StageOverview; // Go to overview to add stages/screws
    }
    void SaveLevel()
    { /* ... (same as V2, calls ValidateV2) ... */
        if (currentLevelData == null || !isDataLoaded) return;
        if (currentLevelData.level != currentLevelNumber || currentLevelData.levelType != currentLevelType) { /* ... mismatch check ... */ return; }
        statusMessage = $"Saving Level {currentLevelData.level}..."; Repaint();
        try
        {
            ValidateLevelDataConsistencyV2(currentLevelData); // Use V2 validation
            LevelDataFactory.UseCompression = useCompression; LevelDataFactory.UseEncryption = useEncryption; LevelDataFactory.LevelsPerChunk = levelsPerChunk;
            LevelDataFactory.SaveLevelData(currentABTestType, currentLevelData);
            isDirty = false; statusMessage = $"Level {currentLevelData.level} saved successfully!";
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            statusMessage = $"Error saving level: {ex.Message}"; Debug.LogError($"Error saving level {currentLevelData.level}: {ex.ToString()}");
            EditorGUILayout.HelpBox(statusMessage, MessageType.Error);
        }
    }

    // --- Init/Validation (Using V2 logic which is correct for the structure) ---
    void InitializeNullArraysV2(LevelData levelData)
    { /* ... (same as V2) ... */
        if (levelData == null) return; if (levelData.stages == null) levelData.stages = new LevelStage[0];
        foreach (var levelStage in levelData.stages)
        {
            if (levelStage == null) continue; if (levelStage.screwDatas == null) levelStage.screwDatas = new ScrewData[0];
            foreach (var screwData in levelStage.screwDatas)
            {
                if (screwData == null) continue; if (screwData.screwStages == null) screwData.screwStages = new ScrewStage[0];
                foreach (var internalScrewStage in screwData.screwStages)
                {
                    if (internalScrewStage == null) continue;
                    ResizeNutDataArrayIfNeeded(internalScrewStage, screwData.size);
                }
            }
        }
    }
    void ValidateLevelDataConsistencyV2(LevelData levelData)
    { /* ... (same as V2) ... */
        if (levelData == null) return;
        foreach (var levelStage in levelData.stages ?? Enumerable.Empty<LevelStage>())
        {
            if (levelStage == null) continue;
            foreach (var screwData in levelStage.screwDatas ?? Enumerable.Empty<ScrewData>())
            {
                if (screwData == null) continue; if (screwData.screwStages == null) screwData.screwStages = new ScrewStage[0];
                foreach (var internalScrewStage in screwData.screwStages)
                {
                    if (internalScrewStage == null) continue;
                    if (internalScrewStage.nutDatas == null || internalScrewStage.nutDatas.Length != screwData.size)
                    {
                        Debug.LogWarning($"Correcting NutData array size for internal stage in screw {screwData.id} before saving.");
                        ResizeNutDataArrayIfNeeded(internalScrewStage, screwData.size); isDirty = true;
                    }
                }
            }
        }
    }

    // --- Copy/Paste Placeholders ---
    void CopyScrewData(ScrewData data)
    {
        if (data == null) return;
        try
        {
            copiedScrewData = new ScrewData(data); // Use copy constructor
            statusMessage = $"Copied Screw {data.id}";
        }
        catch (Exception e) { Debug.LogError($"Failed to copy ScrewData: {e.Message}"); }
    }
    void PasteScrewData(int levelStageIndex, int screwDataIndex)
    {
        LevelStage stage = GetLevelStage(levelStageIndex);
        if (stage == null || copiedScrewData == null || screwDataIndex < 0 || screwDataIndex >= stage.screwDatas.Length) return;
        try
        {
            // Replace existing screw data with a copy of the copied data
            // Keep original ID or assign new? Let's keep for now, user can change.
            stage.screwDatas[screwDataIndex] = new ScrewData(copiedScrewData);
            isDirty = true;
            statusMessage = $"Pasted Screw over index {screwDataIndex}";
            InitializeNullArraysV2(currentLevelData); // Re-validate structure
        }
        catch (Exception e) { Debug.LogError($"Failed to paste ScrewData: {e.Message}"); }
    }
    void CopyScrewStage(ScrewStage data)
    {
        if (data == null) return;
        try
        {
            copiedScrewStage = new ScrewStage(data); // Use copy constructor
            statusMessage = $"Copied Internal Screw Stage";
        }
        catch (Exception e) { Debug.LogError($"Failed to copy ScrewStage: {e.Message}"); }
    }
    void PasteScrewStage(ScrewData parentScrew, int internalStageIndex)
    {
        if (parentScrew == null || parentScrew.screwStages == null || copiedScrewStage == null || internalStageIndex < 0 || internalStageIndex >= parentScrew.screwStages.Length) return;
        try
        {
            // Replace existing internal stage data
            parentScrew.screwStages[internalStageIndex] = new ScrewStage(copiedScrewStage);
            // Ensure pasted stage's nut array matches parent size
            ResizeNutDataArrayIfNeeded(parentScrew.screwStages[internalStageIndex], parentScrew.size);
            isDirty = true;
            statusMessage = $"Pasted Internal Stage over index {internalStageIndex}";
        }
        catch (Exception e) { Debug.LogError($"Failed to paste ScrewStage: {e.Message}"); }
    }

}