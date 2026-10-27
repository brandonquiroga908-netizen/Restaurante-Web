// =================================================================
// PRÁCTICA GUIADA 1 (Unidad 3) — MongoDB: Pedidos de AndesXpress
// Asignatura: Base de Datos III (BDD3T4) — Datos iniciales
// Ejecutar COMPLETO antes de empezar. Es repetible: vuelve a dejar
// la base en su estado inicial cada vez que se ejecuta.
// =================================================================
db = db.getSiblingDB("andesxpress_practica")

// ---------- Limpieza idempotente ----------
db.productos.drop()
db.pedidos.drop()

// ---------- Colección productos (catálogo, referenciada por sku) ----------
db.productos.insertMany([
  { _id: "SKU-001", nombre: "Polera Andina", categoria: "Ropa",
    precio_base: 45.00, stock: 40,
    atributos: { talla: "M", color: "Azul" } },
  { _id: "SKU-002", nombre: "Router WiFi 6", categoria: "Electrónica",
    precio_base: 120.00, stock: 15,
    atributos: { voltaje: "220V", garantia_meses: 12 } },
  { _id: "SKU-003", nombre: "Quinua Orgánica 1kg", categoria: "Alimentos",
    precio_base: 18.50, stock: 100,
    atributos: { fecha_vencimiento: "2026-12-01", lote: "L-2026-04" } },
  { _id: "SKU-004", nombre: "Chompa de Alpaca", categoria: "Ropa",
    precio_base: 210.00, stock: 12,
    atributos: { talla: "L", color: "Gris" } },
  { _id: "SKU-005", nombre: "Audífonos Bluetooth", categoria: "Electrónica",
    precio_base: 85.00, stock: 30,
    atributos: { voltaje: "5V", garantia_meses: 6 } },
  { _id: "SKU-006", nombre: "Café de Yungas 500g", categoria: "Alimentos",
    precio_base: 32.00, stock: 60,
    atributos: { fecha_vencimiento: "2027-03-15", lote: "L-2026-09" } }
])

// ---------- Colección pedidos (los ítems van EMBEBIDOS dentro del pedido) ----------
db.pedidos.insertMany([
  { _id: 1001, cliente: { id: 101, nombre: "Ana Pérez" },
    ciudad: "Santa Cruz", estado: "ENTREGADO",
    fecha: ISODate("2026-10-01T10:00:00Z"),
    items: [ { sku: "SKU-001", cantidad: 2, precio_unit: 45.00 },
             { sku: "SKU-003", cantidad: 1, precio_unit: 18.50 } ],
    total: 108.50 },
  { _id: 1002, cliente: { id: 102, nombre: "Luis Fernández" },
    ciudad: "Cochabamba", estado: "ENTREGADO",
    fecha: ISODate("2026-10-02T11:30:00Z"),
    items: [ { sku: "SKU-002", cantidad: 1, precio_unit: 120.00 } ],
    total: 120.00 },
  { _id: 1003, cliente: { id: 103, nombre: "Marcela Rojas" },
    ciudad: "La Paz", estado: "ENTREGADO",
    fecha: ISODate("2026-10-03T09:15:00Z"),
    items: [ { sku: "SKU-004", cantidad: 1, precio_unit: 210.00 },
             { sku: "SKU-005", cantidad: 2, precio_unit: 85.00 } ],
    total: 380.00 },
  { _id: 1004, cliente: { id: 101, nombre: "Ana Pérez" },
    ciudad: "Santa Cruz", estado: "ENTREGADO",
    fecha: ISODate("2026-10-04T16:45:00Z"),
    items: [ { sku: "SKU-006", cantidad: 3, precio_unit: 32.00 },
             { sku: "SKU-003", cantidad: 2, precio_unit: 18.50 } ],
    total: 133.00 },
  { _id: 1005, cliente: { id: 104, nombre: "Pablo Suárez" },
    ciudad: "Cochabamba", estado: "EN_CAMINO",
    fecha: ISODate("2026-10-06T08:20:00Z"),
    items: [ { sku: "SKU-002", cantidad: 2, precio_unit: 120.00 },
             { sku: "SKU-005", cantidad: 1, precio_unit: 85.00 } ],
    total: 325.00 },
  { _id: 1006, cliente: { id: 102, nombre: "Luis Fernández" },
    ciudad: "Cochabamba", estado: "PENDIENTE",
    fecha: ISODate("2026-10-07T14:00:00Z"),
    items: [ { sku: "SKU-001", cantidad: 1, precio_unit: 45.00 },
             { sku: "SKU-006", cantidad: 1, precio_unit: 32.00 } ],
    total: 77.00 },
  { _id: 1007, cliente: { id: 105, nombre: "Rosa Quispe" },
    ciudad: "La Paz", estado: "PENDIENTE",
    fecha: ISODate("2026-10-08T10:10:00Z"),
    items: [ { sku: "SKU-003", cantidad: 5, precio_unit: 18.50 } ],
    total: 92.50 },
  { _id: 1008, cliente: { id: 103, nombre: "Marcela Rojas" },
    ciudad: "La Paz", estado: "CANCELADO",
    fecha: ISODate("2026-10-08T17:30:00Z"),
    items: [ { sku: "SKU-004", cantidad: 1, precio_unit: 210.00 } ],
    total: 210.00 }
])

// ---------- Verificación de la carga ----------
print("productos: " + db.productos.countDocuments({}) +
      " | pedidos: " + db.pedidos.countDocuments({}))

// =================================================================
// AYUDAS DE VERIFICACIÓN
// Ejecutar el archivo completo con load("ruta/practica-mongodb.js").
// Al repetir el archivo completo, el Anexo A reinicia las colecciones.
// No repetir C2 o C3 aisladamente: volverían a acumular sus cambios.
// =================================================================
var controles = [];
function verificar(tarea, actual, esperado) {
  var correcto = JSON.stringify(actual) === JSON.stringify(esperado);
  controles.push({ tarea: tarea, resultado: actual, correcto: correcto });
  print((correcto ? "[OK] " : "[ERROR] ") + tarea);
  if (!correcto) {
    print("Esperado: " + JSON.stringify(esperado));
    print("Obtenido: " + JSON.stringify(actual));
    throw new Error("Verificación fallida: " + tarea);
  }
}
verificar("Carga", [db.productos.countDocuments({}),
  db.pedidos.countDocuments({})], [6, 8]);

// ---------- A1: Cantidad total de pedidos ----------
var a1 = db.pedidos.countDocuments({});
printjson(a1);
verificar("A1", a1, 8);

// ---------- A2: Pedidos de Cochabamba ----------
var a2 = db.pedidos.find({ ciudad: "Cochabamba" })
  .sort({ _id: 1 }).toArray();
printjson(a2);
verificar("A2", a2.map(p => p._id), [1002, 1005, 1006]);

// ---------- A3: Entregados con total estrictamente mayor que 120 ----------
var a3 = db.pedidos.find({
  estado: "ENTREGADO", total: { $gt: 120 }
}).sort({ _id: 1 }).toArray();
printjson(a3);
verificar("A3", a3.map(p => p._id), [1003, 1004]);

// ---------- A4: Pendientes o en camino con un operador de lista ----------
var a4 = db.pedidos.find({
  estado: { $in: ["PENDIENTE", "EN_CAMINO"] }
}).sort({ _id: 1 }).toArray();
printjson(a4);
verificar("A4", a4.map(p => p._id), [1005, 1006, 1007]);

// ---------- A5: Tres pedidos de mayor total y proyección ----------
var a5 = db.pedidos.find({}, {
  _id: 0, "cliente.nombre": 1, total: 1
}).sort({ total: -1 }).limit(3).toArray();
printjson(a5);
verificar("A5", a5.map(p => [p.cliente.nombre, p.total]),
  [["Marcela Rojas", 380], ["Pablo Suárez", 325],
   ["Marcela Rojas", 210]]);

// ---------- B1: Pedidos que contienen SKU-002 ----------
var b1 = db.pedidos.find({ "items.sku": "SKU-002" })
  .sort({ _id: 1 }).toArray();
printjson(b1);
verificar("B1", b1.map(p => p._id), [1002, 1005]);

// ---------- B2: Comparación con y sin elemMatch ----------
var b2con = db.pedidos.find({
  items: { $elemMatch: { sku: "SKU-003", cantidad: { $gte: 2 } } }
}).sort({ _id: 1 }).toArray();
printjson(b2con);
verificar("B2 con elemMatch", b2con.map(p => p._id), [1004, 1007]);

var b2sin = db.pedidos.find({
  "items.sku": "SKU-003", "items.cantidad": { $gte: 2 }
}).sort({ _id: 1 }).toArray();
printjson(b2sin);
verificar("B2 sin elemMatch", b2sin.map(p => p._id),
  [1001, 1004, 1007]);

// ---------- B3: Productos que poseen el atributo talla ----------
var b3 = db.productos.find({
  "atributos.talla": { $exists: true }
}).sort({ _id: 1 }).toArray();
printjson(b3);
verificar("B3", b3.map(p => p._id), ["SKU-001", "SKU-004"]);

// ---------- C1: Cambiar el estado del pedido 1005 ----------
var c1 = db.pedidos.updateOne(
  { _id: 1005 }, { $set: { estado: "ENTREGADO" } }
);
printjson(c1);
verificar("C1", c1.modifiedCount, 1);
verificar("C1 estado", db.pedidos.findOne({ _id: 1005 }).estado,
  "ENTREGADO");

// ---------- C2: Agregar el ítem y aumentar el total en una sola operación ----------
var c2 = db.pedidos.updateOne(
  { _id: 1006 },
  {
    $push: { items: { sku: "SKU-003", cantidad: 2,
                     precio_unit: 18.50 } },
    $inc: { total: 37.00 }
  }
);
printjson(c2);
var pedido1006 = db.pedidos.findOne({ _id: 1006 });
printjson(pedido1006);
verificar("C2", [c2.modifiedCount, pedido1006.items.length,
  pedido1006.total], [1, 3, 114]);
verificar("C2 item", pedido1006.items[2],
  { sku: "SKU-003", cantidad: 2, precio_unit: 18.50 });

// ---------- C3: Descontar cinco unidades de stock a la categoría Ropa ----------
var c3 = db.productos.updateMany(
  { categoria: "Ropa" }, { $inc: { stock: -5 } }
);
printjson(c3);
var ropa = db.productos.find(
  { categoria: "Ropa" }, { _id: 1, stock: 1 }
).sort({ _id: 1 }).toArray();
printjson(ropa);
verificar("C3", c3.modifiedCount, 2);
verificar("C3 stock", ropa.map(p => [p._id, p.stock]),
  [["SKU-001", 35], ["SKU-004", 7]]);

// ---------- C4: Eliminar 1008 solo si está cancelado y contar ----------
var c4 = db.pedidos.deleteOne({ _id: 1008, estado: "CANCELADO" });
printjson(c4);
var restantes = db.pedidos.countDocuments({});
printjson(restantes);
verificar("C4", [c4.deletedCount, restantes], [1, 7]);

// ---------- D1: Ventas y cantidad de entregados por ciudad ----------
// No recargar el Anexo A: se conserva el estado posterior a C.
var d1 = db.pedidos.aggregate([
  { $match: { estado: "ENTREGADO" } },
  { $group: { _id: "$ciudad", ventas: { $sum: "$total" },
              pedidos: { $sum: 1 } } },
  { $sort: { ventas: -1 } }
]).toArray();
printjson(d1);
verificar("D1", d1.map(p => [p._id, p.ventas, p.pedidos]),
  [["Cochabamba", 445, 2], ["La Paz", 380, 1],
   ["Santa Cruz", 241.5, 2]]);

// ---------- D2: Ticket promedio y cantidad por estado ----------
var d2 = db.pedidos.aggregate([
  { $group: { _id: "$estado", ticketPromedio: { $avg: "$total" },
              pedidos: { $sum: 1 } } },
  { $sort: { _id: 1 } }
]).toArray();
printjson(d2);
verificar("D2", d2.map(p => [p._id, p.ticketPromedio, p.pedidos]),
  [["ENTREGADO", 213.3, 5], ["PENDIENTE", 103.25, 2]]);

// ---------- D3: Tres clientes con mayor suma de compras ----------
// Agrupar por ID evita mezclar clientes que tengan el mismo nombre.
// Se consideran los 7 pedidos restantes, como indica el enunciado.
var d3 = db.pedidos.aggregate([
  { $group: { _id: "$cliente.id",
              nombre: { $first: "$cliente.nombre" },
              compras: { $sum: "$total" }, pedidos: { $sum: 1 } } },
  { $sort: { compras: -1, _id: 1 } },
  { $limit: 3 }
]).toArray();
printjson(d3);
verificar("D3", d3.map(p => [p.nombre, p.compras, p.pedidos]),
  [["Marcela Rojas", 380, 1], ["Pablo Suárez", 325, 1],
   ["Ana Pérez", 241.5, 2]]);

// ---------- E1: Tres productos con más unidades ----------
// Incluir todos los pedidos restantes, también los PENDIENTES.
// Filtrar ENTREGADO aquí no coincidiría con la tabla del documento.
var e1 = db.pedidos.aggregate([
  { $unwind: "$items" },
  { $group: { _id: "$items.sku", unidades: { $sum: "$items.cantidad" } } },
  { $sort: { unidades: -1, _id: 1 } },
  { $limit: 3 }
]).toArray();
printjson(e1);
verificar("E1", e1.map(p => [p._id, p.unidades]),
  [["SKU-003", 10], ["SKU-006", 4], ["SKU-001", 3]]);

// ---------- E2: Crear y listar el índice compuesto ----------
var e2 = db.pedidos.createIndex({ estado: 1, ciudad: 1 });
printjson(e2);
var indices = db.pedidos.getIndexes();
printjson(indices);
verificar("E2", indices.map(i => i.name).sort(),
  ["_id_", "estado_1_ciudad_1"]);
var compuesto = indices.find(i => i.name === "estado_1_ciudad_1");
verificar("E2 campos", compuesto.key, { estado: 1, ciudad: 1 });

// ---------- Resumen final ----------
print("Todas las verificaciones finalizaron correctamente.");
print("Base: " + db.getName());
print("Para capturas: evidencia(\"A\"), evidencia(\"B\"), etc.");

// ---------- Vista de evidencias de esta ejecución ----------
// Conserva resultados verificados de A/B antes de los cambios de C.
// No modifica datos. Solo está disponible tras completar load().
var comandosPorTarea = [
  {"id":"A1","title":"Cantidad total de pedidos","commands":"db.pedidos.countDocuments({})"},
  {"id":"A2","title":"Pedidos de Cochabamba","commands":"db.pedidos.find({ciudad:\"Cochabamba\"}).sort({_id:1}).toArray()"},
  {"id":"A3","title":"Entregados con total estrictamente mayor que 120","commands":"db.pedidos.find({estado:\"ENTREGADO\",total:{$gt:120}}).sort({_id:1}).toArray()"},
  {"id":"A4","title":"Pendientes o en camino con un operador de lista","commands":"db.pedidos.find({estado:{$in:[\"PENDIENTE\",\"EN_CAMINO\"]}}).sort({_id:1}).toArray()"},
  {"id":"A5","title":"Tres pedidos de mayor total y proyección","commands":"db.pedidos.find({},{_id:0,\"cliente.nombre\":1,total:1}).sort({total:-1}).limit(3).toArray()"},
  {"id":"B1","title":"Pedidos que contienen SKU-002","commands":"db.pedidos.find({\"items.sku\":\"SKU-002\"}).sort({_id:1}).toArray()"},
  {"id":"B2","title":"Comparación con y sin elemMatch","commands":"db.pedidos.find({items:{$elemMatch:{sku:\"SKU-003\",cantidad:{$gte:2}}}}).sort({_id:1}).toArray()\ndb.pedidos.find({\"items.sku\":\"SKU-003\",\"items.cantidad\":{$gte:2}}).sort({_id:1}).toArray()"},
  {"id":"B3","title":"Productos que poseen el atributo talla","commands":"db.productos.find({\"atributos.talla\":{$exists:true}}).sort({_id:1}).toArray()"},
  {"id":"C1","title":"Cambiar el estado del pedido 1005","commands":"db.pedidos.updateOne({_id:1005},{$set:{estado:\"ENTREGADO\"}})\ndb.pedidos.findOne({_id:1005}).estado"},
  {"id":"C2","title":"Agregar el ítem y aumentar el total en una sola operación","commands":"db.pedidos.updateOne({_id:1006},{$push:{items:{sku:\"SKU-003\",cantidad:2,precio_unit:18.50}},$inc:{total:37.00}})\ndb.pedidos.findOne({_id:1006})"},
  {"id":"C3","title":"Descontar cinco unidades de stock a la categoría Ropa","commands":"db.productos.updateMany({categoria:\"Ropa\"},{$inc:{stock:-5}})\ndb.productos.find({categoria:\"Ropa\"},{_id:1,stock:1}).sort({_id:1}).toArray()"},
  {"id":"C4","title":"Eliminar 1008 solo si está cancelado y contar","commands":"db.pedidos.deleteOne({_id:1008,estado:\"CANCELADO\"})\ndb.pedidos.countDocuments({})"},
  {"id":"D1","title":"Ventas y cantidad de entregados por ciudad","commands":"db.pedidos.aggregate([{$match:{estado:\"ENTREGADO\"}},{$group:{_id:\"$ciudad\",ventas:{$sum:\"$total\"},pedidos:{$sum:1}}},{$sort:{ventas:-1}}]).toArray()"},
  {"id":"D2","title":"Ticket promedio y cantidad por estado","commands":"db.pedidos.aggregate([{$group:{_id:\"$estado\",ticketPromedio:{$avg:\"$total\"},pedidos:{$sum:1}}},{$sort:{_id:1}}]).toArray()"},
  {"id":"D3","title":"Tres clientes con mayor suma de compras","commands":"db.pedidos.aggregate([{$group:{_id:\"$cliente.id\",nombre:{$first:\"$cliente.nombre\"},compras:{$sum:\"$total\"},pedidos:{$sum:1}}},{$sort:{compras:-1,_id:1}},{$limit:3}]).toArray()"},
  {"id":"E1","title":"Tres productos con más unidades","commands":"db.pedidos.aggregate([{$unwind:\"$items\"},{$group:{_id:\"$items.sku\",unidades:{$sum:\"$items.cantidad\"}}},{$sort:{unidades:-1,_id:1}},{$limit:3}]).toArray()"},
  {"id":"E2","title":"Crear y listar el índice compuesto","commands":"db.pedidos.createIndex({estado:1,ciudad:1})\ndb.pedidos.getIndexes()"}
];
function evidencia(parte) {
  parte = String(parte).toUpperCase();
  if (!["A", "B", "C", "D", "E"].includes(parte)) {
    throw new Error("Use A, B, C, D o E");
  }
  print("\nPARTE " + parte + " | " + db.getName());
  print("Comandos y resumen de valores obtenidos en esta ejecución");
  comandosPorTarea.filter(t => t.id[0] === parte).forEach(t => {
    print("\n" + t.id + " - " + t.title);
    print(t.commands);
    controles.filter(c => c.tarea === t.id ||
      c.tarea.startsWith(t.id + " ")).forEach(c => {
      print("RESULTADO " + c.tarea + ": " + JSON.stringify(c.resultado)
        + (c.correcto ? " [OK]" : " [ERROR]"));
    });
  });
}
