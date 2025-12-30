const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");

// Initialize Express app
const app = express();
const port = 3000;

// Middleware: Enable JSON parsing and CORS
app.use(express.json());
app.use(cors());
// --- DATABASE CONNECTION ---
const dbUrl = process.env.DB_URL || "mongodb://mongo:27017/gamedata";

mongoose
	.connect(dbUrl)
	.then(() => console.log("✅ MongoDB Connected"))
	.catch((err) => console.error("❌ MongoDB Connection Error:", err));

// --- DATA MODEL (SCHEMA) ---
// Define structure for player actions
const ActionSchema = new mongoose.Schema({
	uid: String,
	actionType: String,
	value: String,
	timestamp: { type: Date, default: Date.now },
});

const Action = mongoose.model("Action", ActionSchema);

// --- API ROUTES ---

// 1. Health Check Route (to verify server is running)
app.get("/", (req, res) => {
	res.send("Weapon of Math Destruction Server is Online!");
});

// 2. DATA COLLECTION: Game sends data here (POST)
app.post("/api/track", async (req, res) => {
	try {
		console.log("Data received:", req.body);
		const newAction = new Action(req.body);
		await newAction.save();
		res.json({ status: "success", message: "Data saved successfully" });
	} catch (error) {
		console.error("Save error:", error);
		res.status(500).json({ error: error.message });
	}
});

// 3. PROFILE ANALYSIS: Unity requests profile here (GET)
app.get("/api/profile/:uid", async (req, res) => {
	try {
		const uid = req.params.uid;
		const actions = await Action.find({ uid: uid });
		const aggressiveCount = actions.filter(
			(a) => a.value === "aggressive"
		).length;

		// Determine profile based on threshold
		let profileType = "neutral";
		if (aggressiveCount >= 3) {
			profileType = "hostile";
		}

		// Return the profile to Unity
		console.log(`Analyzing user ${uid}: Classified as ${profileType}`);
		res.json({ uid: uid, trait: profileType });
	} catch (error) {
		res.status(500).json({ error: error.message });
	}
});

// 4. ADMIN DASHBOARD: Serve static files from public folder
app.use("/admin", express.static("public"));

// Start Server
app.listen(port, () => {
	console.log(`🚀 Server running on http://localhost:${port}`);
});
