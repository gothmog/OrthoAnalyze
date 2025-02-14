window.renderD3Tree = function (jsonData, dotNetHelper, height) {
    d3.select("#treeGraph").selectAll("*").remove();
    const data = JSON.parse(jsonData);  // Načteš data z JSONu

    const width = 1600;
    const root = d3.hierarchy(data);
    const dx = 30;  // Zvětšeno pro lepší rozestupy mezi uzly vertikálně
    const dy = width / (root.height + 1);

    const tree = d3.tree().nodeSize([dx, dy]);
    tree(root);

    const svg = d3.select("#treeGraph")
        .attr("width", width)
        .attr("height", height)
        .attr("viewBox", [-dy / 3, -dx, width, 1200])
        .attr("style", "max-width: 100%; height: auto; font: 12px sans-serif;")  // Zvětšeno písmo
        .append("g")
        .attr("transform", "translate(50, 550)");  // Posunuto dolů a doprava

    

    // Vykreslení uzlů
    const node = svg.append("g")
        .selectAll("g")
        .data(root.descendants())
        .enter().append("g")
        .attr("transform", d => `translate(${d.y},${d.x})`)
        .on("click", function (event, d) {
            // Volání Blazor metody při kliknutí na uzel
            dotNetHelper.invokeMethodAsync('LoadSubTree', d.data.name)
                .then(newData => {
                    console.log("New data received:", newData);  // Zobraz data v konzoli
                    renderD3Tree(newData, dotNetHelper);  // Znovu vykresli strom s novými daty
                })
                .catch(error => {
                    console.error("Error invoking LoadSubTree:", error);
                });
        });

    // Přidání hran mezi uzly
    const link = svg.append("g")
        .attr("fill", "none")
        .attr("stroke", "#555")
        .attr("stroke-opacity", 0.6)
        .attr("stroke-width", 1.5)
        .selectAll("path")
        .data(root.links())
        .enter().append("path")
        .attr("d", d3.linkHorizontal()
            .x(d => d.y)
            .y(d => d.x));

    // Zvětšení kolečka uzlů
    node.append("circle")
        .attr("r", 5)  // Zvětšený poloměr
        .attr("fill", d => d.children ? "#555" : "#999")  // Barvy podle toho, zda má děti
        .attr("stroke", "black")
        .attr("stroke-width", 1.5);

    // Zvětšení textu u uzlů
    node.append("text")
        .attr("dy", "0.35em")
        .attr("x", d => d.children ? -10 : 10)  // Posunout text dále od uzlu
        .attr("text-anchor", d => d.children ? "end" : "start")
        .text(d => d.data.name)
        .attr("font-size", d => d.data.Main ? 20 : 11)
        .attr("font-weight", d => d.data.Main ? "bold" : "normal")
        .attr("fill", d => d.data.Main ? "blue" : "black");

    node.filter(d => d.data.selected)
        .append("foreignObject")
        .attr("x", 10)
        .attr("y", -10)
        .attr("width", 80)
        .attr("height", 30)
        .append("xhtml:button")
        .text("Zpět")
        .on("click", function (event, d) {
            event.stopPropagation(); // Zabraň spuštění click eventu na uzlu
            // Vyvolání Blazor metody "OnBackButtonClicked"
            dotNetHelper.invokeMethodAsync('OnBackButtonClicked', d.data.key)
                .then(() => {
                    console.log("Back button clicked for node:", d.data.key);
                })
                .catch(error => {
                    console.error("Error invoking OnBackButtonClicked:", error);
                });
        });
};